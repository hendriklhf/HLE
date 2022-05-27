using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HLE.Twitch.Args;
using HLE.Twitch.Attributes;

namespace HLE.Twitch.Models;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class Channel
{
    public string Name { get; private set; }

    public long Id { get; private set; }

    public bool EmoteOnly { get; private set; }

    public int FollowersOnly { get; private set; }

    public bool R9K { get; private set; }

    public int SlowMode { get; private set; }

    public bool SubsOnly { get; private set; }

    internal Channel(RoomstateArgs args)
    {
        Name = args.Channel;
        Id = args.ChannelId;
        Update(args);
    }

    internal void Update(RoomstateArgs args)
    {
        PropertyInfo[] props = typeof(Channel).GetProperties();
        foreach (PropertyInfo pi in args.ChangedProperties)
        {
            ChannelPropName? propNameAttr = pi.GetCustomAttribute<ChannelPropName>();
            string propName = propNameAttr?.Value ?? throw new ArgumentNullException(nameof(propNameAttr));
            PropertyInfo prop = props.FirstOrDefault(p => p.Name == propName) ?? throw new ArgumentNullException(nameof(prop));
            prop.SetValue(this, pi.GetValue(args));
        }
    }
}
