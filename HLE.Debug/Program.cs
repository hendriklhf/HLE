using System;
using System.Text.Json;
using HLE.Twitch;
using HLE.Twitch.Models;

namespace HLE.Debug;

public static class Program
{
    private static void Main()
    {
        ChatMessage msg = new("@badge-info=subscriber/20;badges=subscriber/12,glitchcon2020/1;color=#5F9EA0;display-name=NotKarar;emotes=175766:0-8;first-msg=0;flags=;historical=1;id=3557dd10-01c0-4d0c-bc95-06956f1113cb;mod=0;rm-received-ts=1651675276145;room-id=11148817;subscriber=1;tmi-sent-ts=1651675274016;turbo=0;user-id=89954186;user-type= :notkarar!notkarar@notkarar.tmi.twitch.tv PRIVMSG #pajlada :forsenWut it's completely strange");
        string json = JsonSerializer.Serialize(msg, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
        Console.ReadLine();
    }
}
