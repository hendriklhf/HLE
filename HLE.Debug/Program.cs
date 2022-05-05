using System;
using HLE.Twitch;

namespace HLE.Debug;

public static class Program
{
    private static void Main()
    {
        TwitchClient client = new();
        client.SetChannels(new[]
        {
            "forsen",
            "xqcow",
            "pokimane"
        });
        client.Connect();
        Console.ReadLine();
    }
}
