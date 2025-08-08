public static class NetKeys
{
    public const string READY = "ready";
    public static string SeatKey(int actorNumber) => $"Seat_{actorNumber}";
    public const string GAME_STARTED = "gameStarted";
}
