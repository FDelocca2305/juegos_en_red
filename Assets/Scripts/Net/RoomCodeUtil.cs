using System;
using System.Linq;

public static class RoomCodeUtil
{
    private static readonly Random _rng = new Random();

    public static string Generate6Digits()
        => _rng.Next(0, 1_000_000).ToString("000000");

    public static bool IsValid(string code)
        => !string.IsNullOrWhiteSpace(code) && code.Length == 6 && code.All(char.IsDigit);
}
