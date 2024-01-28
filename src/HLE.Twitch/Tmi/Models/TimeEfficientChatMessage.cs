using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0660, CS0661, CS0659 // Justification: base class overrides GetHashCode

namespace HLE.Twitch.Tmi.Models;

public sealed class TimeEfficientChatMessage : ChatMessage, IEquatable<TimeEfficientChatMessage>
{
    public override ReadOnlySpan<Badge> BadgeInfos => _badgeInfos.AsSpan(0, _badgeInfoCount);

    public override ReadOnlySpan<Badge> Badges => _badges.AsSpan(0, _badgeCount);

    private readonly Badge[] _badgeInfos;
    private readonly int _badgeInfoCount;
    private readonly Badge[] _badges;
    private readonly int _badgeCount;

    /// <summary>
    /// The default constructor of <see cref="TimeEfficientChatMessage"/>.
    /// </summary>
    /// <param name="badgeInfos">The badge info buffer.</param>
    /// <param name="badgeInfoCount">The amount of badge infos written into the buffer.</param>
    /// <param name="badges">The badge buffer.</param>
    /// <param name="badgeCount">The amount of badges written into the buffer.</param>
    /// <param name="flags">The message flags.</param>
    public TimeEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageFlags flags)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _flags = flags;
    }

    public bool Equals([NotNullWhen(true)] TimeEfficientChatMessage? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public static bool operator ==(TimeEfficientChatMessage? left, TimeEfficientChatMessage? right) => Equals(left, right);

    public static bool operator !=(TimeEfficientChatMessage? left, TimeEfficientChatMessage? right) => !(left == right);
}
