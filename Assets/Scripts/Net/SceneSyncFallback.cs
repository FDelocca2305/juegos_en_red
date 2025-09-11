using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class SceneSyncFallback : MonoBehaviourPunCallbacks
{
    private const string ROOM_LEVEL = "room_level";

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (!changed.ContainsKey(ROOM_LEVEL)) return;

        var target = changed[ROOM_LEVEL] as string;
        if (!string.IsNullOrEmpty(target) && SceneManager.GetActiveScene().name != target)
            PhotonNetwork.LoadLevel(target);
    }

    public override void OnJoinedRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        var props = room.CustomProperties;
        if (props == null) return;

        if (props.TryGetValue(ROOM_LEVEL, out var raw) && raw is string target)
        {
            if (SceneManager.GetActiveScene().name != target)
                PhotonNetwork.LoadLevel(target);
        }
    }
}