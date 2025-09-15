using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGuard : MonoBehaviourPunCallbacks
{
    void Start()
    {
        TryBounceBack();
    }

    public override void OnJoinedRoom() => TryBounceBack();

    private void TryBounceBack()
    {
        if (!PhotonNetwork.InRoom) return;

        string target = null;
        var room = PhotonNetwork.CurrentRoom;
        var props = room?.CustomProperties;
        if (props != null && props.TryGetValue(RoomKeys.ROOM_LEVEL, out var v))
            target = v as string;

        if (string.IsNullOrEmpty(target)) target = "LobbyScene";

        var cur = SceneManager.GetActiveScene().name;
        if (cur != target)
        {
            Debug.Log($"[MenuGuard] InRoom en escena '{cur}'. Volviendo a '{target}'.");
            PhotonNetwork.LoadLevel(target);
        }
    }
}