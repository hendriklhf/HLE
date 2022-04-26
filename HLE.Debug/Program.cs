using System;
using System.Linq;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Twitch;

namespace HLE.Debug;

public static class Program
{
    private static void Main()
    {
        TwitchClient client = new();
        client.SetChannels(new[]
        {
            "lbnshlfe",
            "strbhlfe"
        });
        client.Connect();
        while (true)
        {
            Task.Delay(2500).Wait();
            Console.WriteLine(client.Channels.Count());
            client.Channels.ForEach(c => Console.WriteLine($"{c.Name}: {nameof(c.FollowerOnly)}: {c.FollowerOnly}"));
        }

        //Console.ReadLine();
    }
}
