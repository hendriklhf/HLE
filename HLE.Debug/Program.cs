using System;
using System.Text;
using HLE.Collections;
using HLE.Twitch;
using HLE.Twitch.Models;

namespace HLE.Debug;

#nullable disable

public static class Program
{
    private static void Main()
    {
        TwitchClient client = new()
        {
            ClientType = ClientType.Tcp,
            UseSSL = true
        };

        client.JoinChannels("pietsmiet", "forsen", "xqc", "esfandtv", "summit1g", "crossmauz");
        client.OnChatMessageReceived += (_, msg) => Console.WriteLine($"{msg.Username}: {msg.Message}");
        client.Connect();
        Console.ReadLine();
    }
}
