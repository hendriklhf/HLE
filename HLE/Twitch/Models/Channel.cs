using System.Linq;
using System.Reflection;
using HLE.Twitch.Args;

namespace HLE.Twitch.Models;

public class Channel
{
    public string Name { get; }

    public long Id { get; }

    public bool EmoteOnly { get; private set; }

    public int FollowerOnly { get; private set; }

    public bool R9K { get; private set; }

    public int SlowMode { get; private set; }

    public bool SubOnly { get; private set; }

    internal Channel(RoomstateArgs args)
    {
        Name = args.Channel;
        Id = args.ChannelId;
        Update(args);
    }

    internal void Update(RoomstateArgs args)
    {
        PropertyInfo[] changedProps = RoomstateArgs.IrcProps.Where(p => args.ChangedProperties.Contains(p.Name)).ToArray();
        foreach (PropertyInfo prop in changedProps)
        {
            prop.SetValue(this, prop.GetValue(args));
        }
    }
}
