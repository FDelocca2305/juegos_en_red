using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using Game.Inventory;

public class MatchManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int roundDurationSeconds = 360;
    [SerializeField] private int minPlayersToStart = 4;
    [SerializeField] private bool allowSoloDebug = true;
    
    private bool roundStarted;
    
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var count = PhotonNetwork.PlayerList.Length;
        if (count < minPlayersToStart && !allowSoloDebug)
        {
            Debug.LogWarning($"Jugadores insuficientes ({count}/{minPlayersToStart}).");
            return;
        }

        AssignRolesSafe();
        StartRound();
    }

    private void AssignRolesSafe()
    {
        var players = PhotonNetwork.PlayerList.OrderBy(_ => Random.value).ToList();
        int count = players.Count;
        
        if (count == 1)
        {
            SetRole(players[0], PlayerRole.Detective);
            return;
        }
        
        SetRole(players[0], PlayerRole.Assassin);
        SetRole(players[1], PlayerRole.Detective);
        for (int i = 2; i < count; i++)
            SetRole(players[i], PlayerRole.Innocent);
    }
    
    private void SetRole(Photon.Realtime.Player p, PlayerRole role)
    {
        var ht = new Hashtable
        {
            { NetKeys.ROLE, (byte)role },
            { NetKeys.ALIVE, true },
            { NetKeys.CLUES, 0 }
        };
        p.SetCustomProperties(ht);
        Debug.Log($"[MatchManager] SetRole Actor#{p.ActorNumber} -> {role}");
    }

    private void StartRound()
    {
        double endsAt = PhotonNetwork.Time + roundDurationSeconds;
        var ht = new Hashtable
        {
            { NetKeys.GAME_STARTED, true },
            { NetKeys.ROUND_ENDS_AT, endsAt }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        
        roundStarted = true;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !roundStarted) return;
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(NetKeys.ROUND_ENDS_AT, out var val))
        {
            if (PhotonNetwork.Time >= (double)val)
            {
                EndRound(victoryAssassin:false, reason:"Tiempo agotado");
                return;
            }
        }

        if (!allowSoloDebug)
        {
            var alive = PhotonNetwork.PlayerList
                .Where(p => p.CustomProperties.TryGetValue(NetKeys.ALIVE, out var a) && (bool)a).ToList();

            if (alive.Count < 2) { EndRound(false, "Jugadores insuficientes"); return; }

            bool assassinAlive = alive.Any(p => (byte)p.CustomProperties[NetKeys.ROLE] == (byte)PlayerRole.Assassin);
            bool othersAlive = alive.Any(p => (byte)p.CustomProperties[NetKeys.ROLE] != (byte)PlayerRole.Assassin);

            if (assassinAlive && !othersAlive) { EndRound(true,  "El asesino eliminó a todos"); return; }
            if (!assassinAlive) { EndRound(false, "El asesino murió"); return; }
        }
    }


    private void EndRound(bool victoryAssassin, string reason)
    {
        Debug.Log($"Fin de ronda: {(victoryAssassin ? "Asesino" : "Inocentes/Detective")} - {reason}");
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { NetKeys.GAME_STARTED, false } });
        // TODO: cargar escena de resultados o volver al Lobby
        PhotonNetwork.LoadLevel("LobbyScene");
    }
}
