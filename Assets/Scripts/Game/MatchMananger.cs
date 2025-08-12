using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class MatchManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int roundDurationSeconds = 360;
    [SerializeField] private int minPlayers = 1;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerList.Length < minPlayers)
            {
                Debug.LogWarning("No hay jugadores suficientes.");
                return;
            }

            AssignRoles();
            StartRound();
        }
    }

    private void AssignRoles()
    {
        var players = PhotonNetwork.PlayerList.OrderBy(p => Random.value).ToList();

        var assassin = players[0];
        var detective = players[1];

        SetRole(assassin, PlayerRole.Assassin);
        SetRole(detective, PlayerRole.Detective);

        foreach (var p in players.Skip(2))
            SetRole(p, PlayerRole.Innocent);
    }

    private void SetRole(Player p, PlayerRole role)
    {
        var ht = new Hashtable
        {
            { NetKeys.ROLE, (byte)role },
            { NetKeys.ALIVE, true },
            { NetKeys.CLUES, 0 }
        };
        p.SetCustomProperties(ht);
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
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(NetKeys.ROUND_ENDS_AT, out var val))
        {
            double endsAt = (double)val;
            if (PhotonNetwork.Time >= endsAt)
            {
                EndRound(victoryAssassin: false, reason: "Tiempo agotado");
            }
        }
        
        var alive = PhotonNetwork.PlayerList.Where(p => p.CustomProperties.TryGetValue(NetKeys.ALIVE, out var a) && (bool)a).ToList();
        if (alive.Count > 0)
        {
            bool assassinAlive = alive.Any(p => (byte)p.CustomProperties[NetKeys.ROLE] == (byte)PlayerRole.Assassin);
            bool othersAlive = alive.Any(p => (byte)p.CustomProperties[NetKeys.ROLE] != (byte)PlayerRole.Assassin);
            if (assassinAlive && !othersAlive) EndRound(true, "El asesino eliminó a todos");
            if (!assassinAlive) EndRound(false, "El asesino murió");
        }
    }

    private void EndRound(bool victoryAssassin, string reason)
    {
        Debug.Log($"Fin de ronda: {(victoryAssassin ? "Asesino" : "Inocentes/Detective")} - {reason}");
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { NetKeys.GAME_STARTED, false } });
        // TODO: cargar escena de resultados o volver al Lobby
        PhotonNetwork.LoadLevel("Lobby");
    }
}
