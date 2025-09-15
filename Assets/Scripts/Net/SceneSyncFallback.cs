using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class SceneSyncFallback : MonoBehaviourPunCallbacks
{
    private const string ROOM_LEVEL = "room_level";

    void Start() => TrySyncToRoomLevel();

    public override void OnJoinedRoom() => TrySyncToRoomLevel();

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed != null && changed.ContainsKey(ROOM_LEVEL))
            TrySyncToRoomLevel();
    }

    private void TrySyncToRoomLevel()
    {
        var room = PhotonNetwork.CurrentRoom; 
        if (room?.CustomProperties == null) return;

        if (room.CustomProperties.TryGetValue(ROOM_LEVEL, out var raw) && raw is string target && !string.IsNullOrEmpty(target))
        {
            var cur = SceneManager.GetActiveScene().name;
            if (cur != target)
            {
                UnityEngine.Debug.Log($"[SceneSyncFallback] Sync scene → '{target}' (was '{cur}')");
                PhotonNetwork.LoadLevel(target);
            }
        }
    }
}