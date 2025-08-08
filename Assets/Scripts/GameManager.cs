using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private string playerResourcePath = "NetworkPlayer";

    private void Start()
    {
        int seat = GetSeatOf(PhotonNetwork.LocalPlayer.ActorNumber);
        var spawn = spawnPoints[Mathf.Clamp(seat, 0, spawnPoints.Length - 1)];

        var go = PhotonNetwork.Instantiate(playerResourcePath, spawn.position, spawn.rotation);
    }

    private int GetSeatOf(int actorNumber)
    {
        var key = NetKeys.SeatKey(actorNumber);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out var v))
            return (int)v;
        return 0;
    }
}

