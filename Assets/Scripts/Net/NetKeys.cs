public static class NetKeys
{
    public const string READY = "ready";
    public static string SeatKey(int actorNumber) => $"Seat_{actorNumber}";
    public const string ROLE = "role";
    public const string ALIVE = "alive";
    public const string CLUES = "clues";

    public const string GAME_STARTED = "gameStarted";
    public const string ROUND_ENDS_AT = "roundEndsAt";
    public const string LIGHTS_OFF_UNTIL = "lightsOffUntil";
    public const string DOOR_LOCK_UNTIL_PREFIX = "doorLock_";
}

public enum PlayerRole : byte { Assassin = 0, Detective = 1, Innocent = 2 }
