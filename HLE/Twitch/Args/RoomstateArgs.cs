using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HLE.Twitch.Attributes;

namespace HLE.Twitch.Args;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
public class RoomstateArgs : EventArgs
{
    [IrcTagName("emote-only")]
    public bool EmoteOnly { get; init; }

    [IrcTagName("followers-only")]
    public int FollowersOnly { get; init; }

    [IrcTagName("r9k")]
    public bool R9K { get; init; }

    [IrcTagName("room-id")]
    public long ChannelId { get; init; }

    public string Channel { get; }

    [IrcTagName("slow")]
    public int SlowMode { get; init; }

    [IrcTagName("subs-only")]
    public bool SubsOnly { get; init; }

    internal List<PropertyInfo> ChangedProperties { get; } = new();

    internal static PropertyInfo[] IrcProps { get; } = typeof(RoomstateArgs).GetProperties().Where(p => p.GetCustomAttribute<IrcTagName>() is not null).ToArray();

    private static readonly MethodInfo[] _ircMethods = typeof(RoomstateArgs).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.GetCustomAttribute<MsgPropName>() is not null)
        .ToArray();

    public RoomstateArgs(string ircMessage)
    {
        string[] split = ircMessage.Split();
        string[] roomstateSplit = split[0][1..].Split(';').ToArray();
        Dictionary<string, string> tagDic = roomstateSplit.Select(s => s.Split('=')).ToDictionary(sp => sp[0], sp => sp[1]);

        foreach (PropertyInfo prop in IrcProps)
        {
            IrcTagName attr = prop.GetCustomAttribute<IrcTagName>()!;
            if (!tagDic.TryGetValue(attr.Value, out string? value))
            {
                continue;
            }

            MethodInfo method = _ircMethods.First(m => m.GetCustomAttribute<MsgPropName>()!.Value == prop.Name);
            object? result = method.Invoke(this, new object[]
            {
                value
            });

            if (result is null)
            {
                continue;
            }

            prop.SetValue(this, result);
            ChangedProperties.Add(prop);
        }

        Channel = split[^1][1..];
    }

    [MsgPropName(nameof(EmoteOnly))]
    private bool GetEmoteOnly(string value) => value[^1] == '1';

    [MsgPropName(nameof(FollowersOnly))]
    private int GetFollowersOnly(string value) => value.ToInt();

    [MsgPropName(nameof(R9K))]
    private bool GetR9K(string value) => value[^1] == '1';

    [MsgPropName(nameof(ChannelId))]
    private long GetChannelId(string value) => value.ToLong();

    [MsgPropName(nameof(SlowMode))]
    private int GetSlowMode(string value) => value.ToInt();

    [MsgPropName(nameof(SubsOnly))]
    private bool GetSubsOnly(string value) => value[^1] == '1';
}
