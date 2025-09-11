using ExitGames.Client.Photon;
using Photon.Pun;

public static class RoomKeys
{
    public const string ROOM_LEVEL = "room_level";
    public const string PHASE = "phase";
    public const string ROLES_ASSIGNED = "roles_assigned";
    public const string ALIVE = "alive";
    public const string READY = "scene_ready";
    
    public const int Phase_Lobby = 0;
    public const int Phase_Loading = 1;
    public const int Phase_Playing = 2;
    public const int Phase_Ending = 3;

    public static int GetPhase()
    {
        var r = PhotonNetwork.CurrentRoom;
        if (r?.CustomProperties != null && r.CustomProperties.TryGetValue(PHASE, out var v) && v is int i)
            return i;
        return Phase_Lobby;
    }

    public static void SetPhase(int phase)
    {
        var r = PhotonNetwork.CurrentRoom; if (r == null) return;
        r.SetCustomProperties(new Hashtable { [PHASE] = phase });
    }
}