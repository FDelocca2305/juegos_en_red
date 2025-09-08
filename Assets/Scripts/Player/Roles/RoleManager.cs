using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class RoleManager : MonoBehaviourPunCallbacks, ILocalRoleProvider
{
    public const string RoleKey = "role";
    private const string AssignedKey = "roles_assigned";
    [SerializeField] private int minPlayersForRoles = 2;

    public RoleId LocalRole { get; private set; } = RoleId.Innocent;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient) EnsureRoles();
        UpdateLocal(PhotonNetwork.LocalPlayer);
        ServiceLocator.Register<ILocalRoleProvider>(this);
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryResolve<ILocalRoleProvider>(out var _))
            ServiceLocator.Deregister<ILocalRoleProvider>(this);
    }

    private void EnsureRoles()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        var players = PhotonNetwork.PlayerList.ToList();
        if (players.Count < minPlayersForRoles) return;
        
        var assassins  = players.Where(p => GetRole(p) == RoleId.Assassin).ToList();
        var detectives = players.Where(p => GetRole(p) == RoleId.Detective).ToList();
        var innocents  = players.Where(p => GetRole(p) == RoleId.Innocent).ToList();
        
        if (assassins.Count > 1)
            for (int i = 1; i < assassins.Count; i++) SetRole(assassins[i], RoleId.Innocent);
        if (detectives.Count > 1)
            for (int i = 1; i < detectives.Count; i++) SetRole(detectives[i], RoleId.Innocent);
        
        assassins  = players.Where(p => GetRole(p) == RoleId.Assassin).ToList();
        detectives = players.Where(p => GetRole(p) == RoleId.Detective).ToList();
        innocents  = players.Where(p => GetRole(p) == RoleId.Innocent).ToList();
        
        if (assassins.Count == 0)
        {
            var cand = innocents.Count > 0 ? innocents[Random.Range(0, innocents.Count)] : players[Random.Range(0, players.Count)];
            SetRole(cand, RoleId.Assassin);
            assassins.Add(cand);
            innocents.Remove(cand);
        }
        
        if (detectives.Count == 0)
        {
            var pool = players.Where(p => !assassins.Contains(p)).ToList();
            var cand = pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : players[Random.Range(0, players.Count)];
            SetRole(cand, RoleId.Detective);
            detectives.Add(cand);
            innocents.Remove(cand);
        }
        
        foreach (var p in players)
        {
            var r = GetRole(p);
            if (r != RoleId.Assassin && r != RoleId.Detective)
                SetRole(p, RoleId.Innocent);
        }
        
        var ht = room.CustomProperties ?? new Hashtable();
        ht[AssignedKey] = true;
        room.SetCustomProperties(ht);
    }

    private static RoleId GetRole(Photon.Realtime.Player p)
    {
        if (p.CustomProperties != null && p.CustomProperties.TryGetValue(RoleKey, out var v) && v is int i)
            return (RoleId)i;
        return RoleId.Innocent;
    }

    private static void SetRole(Photon.Realtime.Player p, RoleId role)
    {
        var ht = p.CustomProperties ?? new Hashtable();
        ht[RoleKey] = (int)role;
        p.SetCustomProperties(ht);
    }
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient) EnsureRoles();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient) EnsureRoles();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient) EnsureRoles();
    }
    
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (target == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(RoleKey))
            UpdateLocal(target);
    }

    private void UpdateLocal(Photon.Realtime.Player p)
    {
        if (p.CustomProperties != null &&
            p.CustomProperties.TryGetValue(RoleKey, out var v) && v is int i)
            LocalRole = (RoleId)i;
    }
}
