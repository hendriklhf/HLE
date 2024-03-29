using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0660, CS0661, CS0659 // Justification: base class overrides GetHashCode

namespace HLE.Twitch.Tmi.Models;

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
    /// <param name="badgeInfos">The badge infos.</param>
    /// <param name="badges">The badges.</param>
    /// <param name="flags">The chat message flags.</param>
    public BalancedChatMessage(Badge[] badgeInfos, Badge[] badges, ChatMessageFlags flags)
    {
        _badgeInfos = badgeInfos;
        _badges = badges;
        _flags = flags;
    }

    public bool Equals([NotNullWhen(true)] BalancedChatMessage? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BalancedChatMessage other && Equals(other);

    public static bool operator ==(BalancedChatMessage? left, BalancedChatMessage? right) => Equals(left, right);

    public static bool operator !=(BalancedChatMessage? left, BalancedChatMessage? right) => !(left == right);
}
