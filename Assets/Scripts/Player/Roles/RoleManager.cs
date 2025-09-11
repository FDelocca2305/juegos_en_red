using System.Collections;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-80)]
public class RoleManager : MonoBehaviourPunCallbacks, ILocalRoleProvider
{
    public const string RoleKey     = "role";
    public const string AssignedKey = "roles_assigned";

    [SerializeField] private int   minPlayersForRoles = 2;
    [SerializeField] private float assignDelaySeconds = 10f;
    [SerializeField] private float retryTimeout = 25f;

    public RoleId LocalRole { get; private set; } = RoleId.Innocent;

    private Coroutine _assignRoutine;
    
    private static bool TryToInt(object o, out int val)
    {
        switch (o)
        {
            case int i:     val = i; return true;
            case byte b:    val = b; return true;
            case sbyte sb:  val = sb; return true;
            case short s:   val = s; return true;
            case ushort us: val = us; return true;
            case uint ui:   val = (int)ui; return true;
            case long l:    val = (int)l; return true;
            case ulong ul:  val = (int)ul; return true;
            case float f:   val = (int)f; return true;
            case double d:  val = (int)d; return true;
            default:
                try { val = System.Convert.ToInt32(o); return true; }
                catch { val = 0; return false; }
        }
    }

    public override void OnEnable()
    {
        Debug.Log($"[RoleManager] OnEnable. IsMaster={PhotonNetwork.IsMasterClient}, Phase={RoomKeys.GetPhase()}");
        if (PhotonNetwork.IsMasterClient) EnsureAssignRoutine();
    }

    private void Start()
    {
        UpdateLocal(PhotonNetwork.LocalPlayer);
        ServiceLocator.Register<ILocalRoleProvider>(this);
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.F8))
        {
            Debug.LogWarning("[RoleManager] F8 pressed -> forcing TryAssignIfNeeded()");
            TryAssignIfNeeded(true);
        }
    }

    private void EnsureAssignRoutine()
    {
        if (_assignRoutine == null)
        {
            _assignRoutine = StartCoroutine(AssignWithDelay());
            Debug.Log("[RoleManager] Assign coroutine started.");
        }
    }

    private IEnumerator AssignWithDelay()
    {
        while (RoomKeys.GetPhase() != RoomKeys.Phase_Loading) { yield return null; }
        Debug.Log("[RoleManager] Phase=Loading detected. Starting delay window...");

        float t0 = Time.time;
        
        while (PhotonNetwork.PlayerList.Length < minPlayersForRoles &&
               Time.time - t0 < assignDelaySeconds)
        {
            Debug.Log($"[RoleManager] Waiting players... {PhotonNetwork.PlayerList.Length}/{minPlayersForRoles}");
            yield return new WaitForSeconds(0.25f);
        }
        
        float remaining = assignDelaySeconds - (Time.time - t0);
        if (remaining > 0f)
        {
            Debug.Log($"[RoleManager] Completing delay: {remaining:0.00}s");
            yield return new WaitForSeconds(remaining);
        }
        
        float end = Time.time + retryTimeout;
        while (RoomKeys.GetPhase() == RoomKeys.Phase_Loading &&
               !RolesAlreadyAssigned() &&
               Time.time < end)
        {
            TryAssignIfNeeded(false);
            if (RolesAlreadyAssigned()) break;
            yield return new WaitForSeconds(0.5f);
        }

        if (!RolesAlreadyAssigned())
            Debug.LogError("[RoleManager] Gave up trying to assign roles (timeout).");
        else
            Debug.Log("[RoleManager] Roles are assigned. Exiting coroutine.");
    }

    private static bool RolesAlreadyAssigned()
    {
        var room = PhotonNetwork.CurrentRoom;
        return room?.CustomProperties != null &&
               room.CustomProperties.TryGetValue(AssignedKey, out var v) &&
               v is bool b && b;
    }

    private void TryAssignIfNeeded(bool forcedLog)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (forcedLog) Debug.LogWarning("[RoleManager] Not master, ignoring TryAssignIfNeeded.");
            return;
        }

        int phase = RoomKeys.GetPhase();
        if (phase != RoomKeys.Phase_Loading)
        {
            if (forcedLog) Debug.LogWarning($"[RoleManager] Phase != Loading ({phase}), skip.");
            return;
        }

        var room = PhotonNetwork.CurrentRoom;
        if (room == null)
        {
            if (forcedLog) Debug.LogWarning("[RoleManager] No room.");
            return;
        }

        if (RolesAlreadyAssigned())
        {
            if (forcedLog) Debug.LogWarning("[RoleManager] Already assigned.");
            return;
        }

        var players = PhotonNetwork.PlayerList.ToList();
        if (players.Count < minPlayersForRoles)
        {
            if (forcedLog) Debug.LogWarning($"[RoleManager] Not enough players to assign roles ({players.Count}/{minPlayersForRoles}).");
            return;
        }
        
        var assassin = players[Random.Range(0, players.Count)];
        players.Remove(assassin);
        var detective = players.Count > 0 ? players[Random.Range(0, players.Count)] : assassin;
        
        room.SetCustomProperties(new Hashtable {
            [AssignedKey] = true,
            [RoomKeys.PHASE] = RoomKeys.Phase_Playing,
            [RoomRoleKeys.ASSASSIN_ACTOR] = assassin.ActorNumber,
            [RoomRoleKeys.DETECTIVE_ACTOR] = detective.ActorNumber
        });
        
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var roleForP =
                (p.ActorNumber == assassin.ActorNumber)  ? RoleId.Assassin :
                (p.ActorNumber == detective.ActorNumber) ? RoleId.Detective :
                                                            RoleId.Innocent;

            p.SetCustomProperties(new Hashtable { [RoleKey] = (int)roleForP });
        }

        TryHydrateLocalFromRoom();

        Debug.Log($"[RoleManager] ASSIGNED -> Assassin={assassin.NickName} | Detective={detective.NickName} | Others=Innocent");
    }

    public override void OnRoomPropertiesUpdate(Hashtable changedProps)
    {
        TryHydrateLocalFromRoom();
    }

    public override void OnJoinedRoom()
    {
        TryHydrateLocalFromRoom();

        var room = PhotonNetwork.CurrentRoom;
        if (PhotonNetwork.IsMasterClient && room?.CustomProperties != null &&
            room.CustomProperties.TryGetValue(AssignedKey, out var a) && a is bool ok && ok)
        {
            var me = PhotonNetwork.LocalPlayer;

            if (me.CustomProperties == null || !me.CustomProperties.ContainsKey(RoleKey))
                me.SetCustomProperties(new Hashtable { [RoleKey] = (int)RoleId.Innocent });
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (target == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(RoleKey))
            UpdateLocal(target);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && RolesAlreadyAssigned())
        {
            newPlayer.SetCustomProperties(new Hashtable { [RoleKey] = (int)RoleId.Innocent });
        }

        if (PhotonNetwork.IsMasterClient && RoomKeys.GetPhase() == RoomKeys.Phase_Loading)
            EnsureAssignRoutine();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient && RoomKeys.GetPhase() == RoomKeys.Phase_Loading)
            EnsureAssignRoutine();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient && RoomKeys.GetPhase() == RoomKeys.Phase_Loading)
            EnsureAssignRoutine();
    }

    private void UpdateLocal(Photon.Realtime.Player p)
    {
        if (p?.CustomProperties != null && p.CustomProperties.TryGetValue(RoleKey, out var v) && TryToInt(v, out var i))
        {
            LocalRole = (RoleId)i;
            Debug.Log($"[RoleManager] Local role = {LocalRole}");
        }
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryResolve<ILocalRoleProvider>(out var _))
            ServiceLocator.Deregister<ILocalRoleProvider>(this);
    }

    private void TryHydrateLocalFromRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room?.CustomProperties == null) return;

        if (room.CustomProperties.TryGetValue(AssignedKey, out var a) && a is bool ok && ok)
        {
            int me = PhotonNetwork.LocalPlayer.ActorNumber;

            int assassinActor = -1, detectiveActor = -1;
            if (room.CustomProperties.TryGetValue(RoomRoleKeys.ASSASSIN_ACTOR, out var ra))
                TryToInt(ra, out assassinActor);
            if (room.CustomProperties.TryGetValue(RoomRoleKeys.DETECTIVE_ACTOR, out var rd))
                TryToInt(rd, out detectiveActor);

            RoleId target =
                (assassinActor == me)  ? RoleId.Assassin :
                (detectiveActor == me) ? RoleId.Detective :
                                         RoleId.Innocent;

            int cur = (int)(PhotonNetwork.LocalPlayer.CustomProperties?[RoleKey] ?? 0);
            if (cur != (int)target)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { [RoleKey] = (int)target });
            }

            LocalRole = target;
            Debug.Log($"[RoleManager] Hydrate → me={me} assassin={assassinActor} detective={detectiveActor} → LocalRole={LocalRole}");
        }
    }
}

public static class RoomRoleKeys
{
    public const string ASSASSIN_ACTOR  = "assassin_actor";
    public const string DETECTIVE_ACTOR = "detective_actor";
}
