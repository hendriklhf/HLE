using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HLE.Twitch.Args;
using HLE.Twitch.Attributes;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a channel with all its room states.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public sealed class Channel
{
    /// <summary>
    /// The username of the channel owner. All lower case.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    /// Indicates whether emote-only mode is turned on or off.
    /// </summary>
    public bool EmoteOnly { get; private set; }

    /// <summary>
    /// Indicates whether followers-only mode is turned on or off.
    /// Value is "-1" if turned off, otherwise the value indicates the number of minutes a user has to follow the channel in order to be able to send messages.
    /// </summary>
    public int FollowersOnly { get; private set; }

    /// <summary>
    /// Indicates whether R9K mode is turned on or off.
    /// </summary>
    public bool R9K { get; private set; }

    /// <summary>
    /// Indicates whether slow mode os turned on or off.
    /// Value is "-1" if turned off, otherwise the value indicates the number of seconds between each message a user can send.
    /// </summary>
    public int SlowMode { get; private set; }

    /// <summary>
    /// Indicates whether subs-only mode is turned on or off.
    /// </summary>
    public bool SubsOnly { get; private set; }

    private static readonly PropertyInfo[] _properties = typeof(Channel).GetProperties();

    internal Channel(RoomstateArgs args)
    {
        Name = args.Channel;
        Id = args.ChannelId;
        EmoteOnly = args.EmoteOnly;
        FollowersOnly = args.FollowersOnly;
        R9K = args.R9K;
        SlowMode = args.SlowMode;
        SubsOnly = args.SubsOnly;
    }

    internal void Update(RoomstateArgs args)
    {
        foreach (PropertyInfo pi in args.ChangedProperties)
        {
            ChannelPropName? propNameAttr = pi.GetCustomAttribute<ChannelPropName>();
            string propName = propNameAttr?.Value ?? throw new ArgumentNullException(nameof(propNameAttr));
            PropertyInfo prop = _properties.FirstOrDefault(p => p.Name == propName) ?? throw new ArgumentNullException(nameof(prop));
            prop.SetValue(this, pi.GetValue(args));
        }
    }
}
