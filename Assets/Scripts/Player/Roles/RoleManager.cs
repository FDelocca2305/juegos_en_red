using System.Collections;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-80)]
[RequireComponent(typeof(PhotonView))]
public class RoleManager : MonoBehaviourPunCallbacks, ILocalRoleProvider
{
    public const string RoleKey     = "role";
    public const string AssignedKey = "roles_assigned";

    [SerializeField] private int   minPlayersForRoles = 2;
    [SerializeField] private float courtesyDelay      = 0.75f;
    [SerializeField] private float readyWaitTimeout   = 20f;
    [SerializeField] private float retryTimeout       = 25f;
    [SerializeField] private float pollInterval       = 0.5f;

    public RoleId LocalRole { get; private set; } = RoleId.Innocent;
    private Coroutine _assignRoutine;

    public override void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient) EnsureAssignRoutine();
    }

    private void Start()
    {
        UpdateLocal(PhotonNetwork.LocalPlayer);
        ServiceLocator.Register<ILocalRoleProvider>(this);
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryResolve<ILocalRoleProvider>(out var _))
            ServiceLocator.Deregister<ILocalRoleProvider>(this);
    }

    private void EnsureAssignRoutine()
    {
        if (_assignRoutine == null)
            _assignRoutine = StartCoroutine(AssignRoutine());
    }

    private IEnumerator AssignRoutine()
    {
        while (!PhotonNetwork.InRoom) yield return null;

        string scene = SceneManager.GetActiveScene().name;
        while (!RoomLevelIs(scene)) yield return null;

        if (courtesyDelay > 0f) yield return new WaitForSeconds(courtesyDelay);

        float readyDeadline = Time.time + readyWaitTimeout;
        
        while (PhotonNetwork.PlayerList.Length < minPlayersForRoles && Time.time < readyDeadline)
        {
            yield return new WaitForSeconds(0.25f);
        }
        
        while (!AllPlayersReady() && Time.time < readyDeadline)
        {
            yield return new WaitForSeconds(0.25f);
        }
        
        float end = Time.time + retryTimeout;
        while (!RolesAlreadyAssigned() && Time.time < end)
        {
            TryAssignOnce();
            if (RolesAlreadyAssigned()) break;
            yield return new WaitForSeconds(pollInterval);
        }

        if (!RolesAlreadyAssigned())
            Debug.LogError("[RoleManager] Gave up trying to assign roles (timeout).");

        _assignRoutine = null;
    }

    private bool RoomLevelIs(string sceneName)
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room?.CustomProperties == null) return false;
        if (!room.CustomProperties.TryGetValue(RoomKeys.ROOM_LEVEL, out var v)) return false;
        return (v as string) == sceneName;
    }

    private static bool RolesAlreadyAssigned()
    {
        var room = PhotonNetwork.CurrentRoom;
        return room?.CustomProperties != null &&
               room.CustomProperties.TryGetValue(AssignedKey, out var v) &&
               v is bool b && b;
    }

    private bool AllPlayersReady()
        => PhotonNetwork.PlayerList.All(p => (bool)(p.CustomProperties?[RoomKeys.READY] ?? false));

    private void TryAssignOnce()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.PlayerList.Length < minPlayersForRoles) return;
        if (RolesAlreadyAssigned()) return;

        var players   = PhotonNetwork.PlayerList.ToList();
        var assassin  = players[Random.Range(0, players.Count)];
        players.Remove(assassin);
        var detective = players[Random.Range(0, players.Count)];
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable {
            [AssignedKey]                  = true,
            [RoomKeys.PHASE]               = RoomKeys.Phase_Playing,
            [RoomRoleKeys.ASSASSIN_ACTOR]  = assassin.ActorNumber,
            [RoomRoleKeys.DETECTIVE_ACTOR] = detective.ActorNumber
        });

        photonView.RPC(nameof(RPC_ApplyAssignedRoles),
                       RpcTarget.AllBuffered,
                       assassin.ActorNumber,
                       detective.ActorNumber);

        Debug.Log($"[RoleManager] ASSIGNED → Assassin={assassin.NickName} | Detective={detective.NickName}");
    }

    [PunRPC]
    private void RPC_ApplyAssignedRoles(int assassinActor, int detectiveActor, PhotonMessageInfo info)
    {
        int me = PhotonNetwork.LocalPlayer.ActorNumber;
        RoleId role = (me == assassinActor) ? RoleId.Assassin :
                      (me == detectiveActor) ? RoleId.Detective :
                                               RoleId.Innocent;
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { [RoleKey] = (int)role });

        LocalRole = role;
        Debug.Log($"[RoleManager] RPC apply → me={me} assassin={assassinActor} detective={detectiveActor} ⇒ {role}");
    }
    
    public override void OnRoomPropertiesUpdate(Hashtable changedProps) => TryHydrateLocalFromRoom();
    public override void OnJoinedRoom() { TryHydrateLocalFromRoom(); if (PhotonNetwork.IsMasterClient) EnsureAssignRoutine(); }
    public override void OnMasterClientSwitched(Photon.Realtime.Player _) { if (PhotonNetwork.IsMasterClient && !RolesAlreadyAssigned()) EnsureAssignRoutine(); }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player _) { if (PhotonNetwork.IsMasterClient && !RolesAlreadyAssigned()) EnsureAssignRoutine(); }
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (target == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(RoleKey))
            UpdateLocal(target);
    }

    private void UpdateLocal(Photon.Realtime.Player p)
    {
        if (p?.CustomProperties != null &&
            p.CustomProperties.TryGetValue(RoleKey, out var v) &&
            TryToInt(v, out var i))
        {
            LocalRole = (RoleId)i;
        }
    }

    private void TryHydrateLocalFromRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room?.CustomProperties == null) return;

        if (room.CustomProperties.TryGetValue(AssignedKey, out var a) && a is bool ok && ok)
        {
            int me = PhotonNetwork.LocalPlayer.ActorNumber;

            int assassinActor = -1, detectiveActor = -1;
            if (room.CustomProperties.TryGetValue(RoomRoleKeys.ASSASSIN_ACTOR, out var ra)) TryToInt(ra, out assassinActor);
            if (room.CustomProperties.TryGetValue(RoomRoleKeys.DETECTIVE_ACTOR, out var rd)) TryToInt(rd, out detectiveActor);

            RoleId target = (me == assassinActor) ? RoleId.Assassin :
                            (me == detectiveActor) ? RoleId.Detective :
                                                     RoleId.Innocent;

            int cur = (int)(PhotonNetwork.LocalPlayer.CustomProperties?[RoleKey] ?? 0);
            if (cur != (int)target)
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { [RoleKey] = (int)target });

            LocalRole = target;
        }
    }

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
}

public static class RoomRoleKeys
{
    public const string ASSASSIN_ACTOR  = "assassin_actor";
    public const string DETECTIVE_ACTOR = "detective_actor";
}
