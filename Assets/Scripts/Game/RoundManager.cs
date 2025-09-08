using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[DefaultExecutionOrder(-40)]
public class RoundManager : MonoBehaviourPunCallbacks, IRoundService
{
    [SerializeField] private string lobbySceneName = "MenuScene";
    [SerializeField] private float endDelay = 4f;

    const string ALIVE = "alive";
    const string WINNER = "winner";
    const string ROUND_OVER = "round_over";

    private void Awake() => ServiceLocator.Register<IRoundService>(this);
    private void OnDestroy() { if (ServiceLocator.TryResolve<IRoundService>(out _)) ServiceLocator.Deregister<IRoundService>(this); }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!changedProps.ContainsKey(ALIVE)) return;

        Evaluate();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var room = PhotonNetwork.CurrentRoom;
            var ht = room.CustomProperties ?? new Hashtable();
            ht[ROUND_OVER] = false;
            room.SetCustomProperties(ht);
        }
    }

    private void Evaluate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool assassinAlive = false;
        int othersAlive = 0;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            var role = (RoleId)((int)(p.CustomProperties?[RoleManager.RoleKey] ?? (int)RoleId.Innocent));
            bool alive = (bool)(p.CustomProperties?[ALIVE] ?? true);

            if (!alive) continue;

            if (role == RoleId.Assassin) assassinAlive = true;
            else othersAlive++;
        }

        if (!assassinAlive)
            photonView.RPC(nameof(RPC_EndRound), RpcTarget.All, "INNOCENTS", "Assassin muerto");
        else if (othersAlive == 0)
            photonView.RPC(nameof(RPC_EndRound), RpcTarget.All, "ASSASSIN", "Todos eliminados");
    }

    public void EndRound(string winner, string reason)
        => photonView.RPC(nameof(RPC_EndRound), RpcTarget.All, winner, reason);

    [PunRPC]
    private void RPC_EndRound(string winner, string reason)
    {
        var ui = ServiceLocator.TryResolve<UI.Gameplay.IGameplayUI>(out var g) ? g : null;
        ui?.ShowHint($"{winner} WIN\n{reason}", 3f);

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(LoadLobbyAfter(endDelay));
    }

    private System.Collections.IEnumerator LoadLobbyAfter(float s)
    {
        yield return new WaitForSeconds(s);
        PhotonNetwork.LoadLevel(lobbySceneName);
    }
}
