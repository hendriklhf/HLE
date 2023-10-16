using System;

namespace HLE.Twitch.Models;

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
    public TimeEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageTags tags)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _tags = tags;
    }

    public bool Equals(TimeEfficientChatMessage? other) => ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);

    public override bool Equals(object? obj) => obj is TimeEfficientChatMessage other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Id, TmiSentTs);

    public static bool operator ==(TimeEfficientChatMessage? left, TimeEfficientChatMessage? right) => Equals(left, right);

    public static bool operator !=(TimeEfficientChatMessage? left, TimeEfficientChatMessage? right) => !(left == right);
}
