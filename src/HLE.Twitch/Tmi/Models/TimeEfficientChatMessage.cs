using System;
using System.Diagnostics.CodeAnalysis;

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
    public TimeEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageFlags flags)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _flags = flags;
    }

    public bool Equals([NotNullWhen(true)] TimeEfficientChatMessage? other) => ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is TimeEfficientChatMessage other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Id, TmiSentTs);

    public static bool operator ==(TimeEfficientChatMessage? left, TimeEfficientChatMessage? right) => Equals(left, right);

    public static bool operator !=(TimeEfficientChatMessage? left, TimeEfficientChatMessage? right) => !(left == right);
}
