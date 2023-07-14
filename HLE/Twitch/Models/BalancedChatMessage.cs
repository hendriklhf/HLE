using System;
using System.Diagnostics;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a balanced chat message.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class BalancedChatMessage : ChatMessage, IEquatable<BalancedChatMessage>
{
    public override ReadOnlySpan<Badge> BadgeInfos => _badgeInfos;

    public override ReadOnlySpan<Badge> Badges => _badges;

    private readonly Badge[]? _badgeInfos;
    private readonly Badge[]? _badges;

    /// <summary>
    /// The default constructor of <see cref="BalancedChatMessage"/>.
    /// </summary>
    public BalancedChatMessage(Badge[] badgeInfos, Badge[] badges, ChatMessageTags tags)
    {
        _badgeInfos = badgeInfos;
        _badges = badges;
        _tags = tags;
    }

    public bool Equals(BalancedChatMessage? other)
    {
        return ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);
    }

    public override bool Equals(object? obj)
    {
        return obj is BalancedChatMessage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, TmiSentTs);
    }

    public static bool operator ==(BalancedChatMessage? left, BalancedChatMessage? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BalancedChatMessage? left, BalancedChatMessage? right)
    {
        return !(left == right);
    }
}
