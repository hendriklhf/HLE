using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using HLE.Collections;
using HLE.Memory;

#pragma warning disable CS0660, CS0661, CS0659 // Justification: base class overrides GetHashCode

namespace HLE.Twitch.Tmi.Models;

public sealed class MemoryEfficientChatMessage : ChatMessage, IEquatable<MemoryEfficientChatMessage>
{
    public override ReadOnlySpan<Badge> BadgeInfos
    {
        get
        {
            if (_badgeInfos is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            return _badgeInfos.AsSpanUnsafe(.._badgeInfoCount);
        }
    }

    public override ReadOnlySpan<Badge> Badges
    {
        get
        {
            if (_badges is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            return _badges.AsSpanUnsafe(.._badgeCount);
        }
    }

    private Badge[]? _badgeInfos;
    private readonly int _badgeInfoCount;

    private Badge[]? _badges;
    private readonly int _badgeCount;

    /// <summary>
    /// The default constructor of <see cref="MemoryEfficientChatMessage"/>.
    /// </summary>
    /// <param name="badgeInfos">The badge info buffer.</param>
    /// <param name="badgeInfoCount">The amount of written elements in the badge info buffer.</param>
    /// <param name="badges">The badge buffer.</param>
    /// <param name="badgeCount">The amount of written elements in the badge buffer.</param>
    /// <param name="flags">The chat message flags.</param>
    public MemoryEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageFlags flags)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _flags = flags;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ArrayPool<Badge>.Shared.Return(_badgeInfos);
        _badgeInfos = null;

        ArrayPool<Badge>.Shared.Return(_badges);
        _badges = null;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] MemoryEfficientChatMessage? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public static bool operator ==(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => Equals(left, right);

    public static bool operator !=(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => !(left == right);
}
