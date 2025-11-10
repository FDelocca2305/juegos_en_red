using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

[DefaultExecutionOrder(-40)]
[RequireComponent(typeof(PhotonView))]
public class RoundManager : MonoBehaviourPunCallbacks, IRoundService
{
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private float endDelay = 4f;

    const string ALIVE = "alive";
    const string WINNER = "winner";
    const string ROUND_OVER = "round_over";

    //Leaderboard score only assasin
    [SerializeField] private string assassinLeaderboardKey = "wins_assasins_round";
    [SerializeField] private int assassinWinScore = 1;

    [SerializeField] private string innocentLeaderboardKey = "wins_innocent_round";
    [SerializeField] private int innocentWinScore = 1;
    private void Awake() => ServiceLocator.Register<IRoundService>(this);
    private void OnDestroy() { if (ServiceLocator.TryResolve<IRoundService>(out _)) ServiceLocator.Deregister<IRoundService>(this); }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (RoomKeys.GetPhase() != RoomKeys.Phase_Playing) return;
        if (!changedProps.ContainsKey(RoomKeys.ALIVE)) return;
        Evaluate();
    }

    private void Evaluate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (RoomKeys.GetPhase() != RoomKeys.Phase_Playing) return;

        var room = PhotonNetwork.CurrentRoom;
        if (room?.CustomProperties == null) return;
        
        if (!(room.CustomProperties.TryGetValue(RoleManager.AssignedKey, out var asg) && asg is bool ok && ok)) return;
        if (!(room.CustomProperties.TryGetValue(RoomRoleKeys.ASSASSIN_ACTOR, out var ra) && ra is int assassinActor)) return;

        bool assassinAlive = PhotonNetwork.PlayerList
            .Any(p => p.ActorNumber == assassinActor && (bool)(p.CustomProperties?[RoomKeys.ALIVE] ?? true));

        int othersAlive = PhotonNetwork.PlayerList
            .Where(p => p.ActorNumber != assassinActor)
            .Count(p => (bool)(p.CustomProperties?[RoomKeys.ALIVE] ?? true));

        if (!assassinAlive)
            photonView.RPC(nameof(RPC_EndRound), RpcTarget.All, "INNOCENTS", "Assassin dead");
        else if (othersAlive == 0)
            photonView.RPC(nameof(RPC_EndRound), RpcTarget.All, "ASSASSIN", "All dead");
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (RoomKeys.GetPhase() != RoomKeys.Phase_Playing) return;
        Evaluate();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (RoomKeys.GetPhase() == RoomKeys.Phase_Playing)
            Invoke(nameof(Evaluate), 0.2f);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { [ROUND_OVER] = false });
        }
    }

    public void EndRound(string winner, string reason)
        => photonView.RPC(nameof(RPC_EndRound), RpcTarget.All, winner, reason);

    [PunRPC]
    public void RPC_EndRound(string winner, string reason)
    {
        ServiceLocator.TryResolve(out UI.Gameplay.IGameplayUI ui);
        ui?.ShowHint($"{winner} WIN\n{reason}", 3f);

        var room = PhotonNetwork.CurrentRoom;
        if (room != null)
            room.SetCustomProperties(new Hashtable { [ROUND_OVER] = true });

        RoomKeys.SetPhase(RoomKeys.Phase_Ending);
        
        if (PhotonNetwork.IsMasterClient)
        {
            var stats = new Hashtable
            {
                ["last_winner"] = winner,
                ["rounds_played"] = (int)(room.CustomProperties?["rounds_played"] ?? 0) + 1
            };
            
            if (winner == "ASSASSIN")
                stats["assassin_wins"] = (int)(room.CustomProperties?["assassin_wins"] ?? 0) + 1;
            else if (winner == "INNOCENTS")
                stats["innocent_wins"] = (int)(room.CustomProperties?["innocent_wins"] ?? 0) + 1;

            //Leaderboard score only assasin
            if (winner == "ASSASSIN" && !string.IsNullOrEmpty(assassinLeaderboardKey))
            {
                if (LootLockerBootsStrap.SessionStarted)
                {
                    LeaderboardService.SubmitScore(assassinWinScore, assassinLeaderboardKey);
                }
                else
                {
                    StartCoroutine(SubmitAssassinWinWhenReady());
                }
            }
            if (winner == "INNOCENTS" && !string.IsNullOrEmpty(innocentLeaderboardKey))
            {
                if (LootLockerBootsStrap.SessionStarted)
                {
                    LeaderboardService.SubmitScore(innocentWinScore, innocentLeaderboardKey);
                }
                else
                {
                    StartCoroutine(SubmitAssassinWinWhenReady());
                }
            }
            room.SetCustomProperties(stats);
        }
        
        RPC_ResetLocalPlayerProps();
        
        Cursor.lockState = CursorLockMode.None;
        
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(LoadLobbyAfter(endDelay));
    }

    private System.Collections.IEnumerator LoadLobbyAfter(float s)
    {
        yield return new WaitForSeconds(s);

        if (!PhotonNetwork.IsMasterClient) yield break;

        var room = PhotonNetwork.CurrentRoom;
        var ht = room.CustomProperties ?? new Hashtable();

        ht[RoomKeys.PHASE] = RoomKeys.Phase_Lobby;
        ht[RoleManager.AssignedKey] = false;
        ht[RoomKeys.ROOM_LEVEL] = lobbySceneName;

        room.IsOpen = true;
        room.IsVisible = true;
        room.SetCustomProperties(ht);
        
    }

    
    private void RPC_ResetLocalPlayerProps()
    {
        var ht = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
        ht[RoomKeys.ALIVE] = true;
        ht[RoomKeys.READY] = null;
        ht[RoleManager.RoleKey] = null;
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
    }

        private System.Collections.IEnumerator SubmitAssassinWinWhenReady()
    {
        while (!LootLockerBootsStrap.SessionStarted)
            yield return null;

        LeaderboardService.SubmitScore(assassinWinScore, assassinLeaderboardKey);
    }
}
