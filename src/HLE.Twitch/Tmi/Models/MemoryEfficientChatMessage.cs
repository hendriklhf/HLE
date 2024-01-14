using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Twitch.Tmi.Models;

public sealed class MemoryEfficientChatMessage : ChatMessage, IEquatable<MemoryEfficientChatMessage>
{
    public override ReadOnlySpan<Badge> BadgeInfos
    {
        get
        {
            ObjectDisposedException.ThrowIf(_badgeInfos is null, typeof(MemoryEfficientChatMessage));
            return _badgeInfos.AsSpan(.._badgeInfoCount);
        }
    }

    public override ReadOnlySpan<Badge> Badges
    {
        get
        {
            ObjectDisposedException.ThrowIf(_badges is null, typeof(MemoryEfficientChatMessage));
            return _badges.AsSpan(.._badgeCount);
        }
    }

    public override required string DisplayName
    {
        get
        {
            string? displayName = _displayName;
            if (displayName is not null)
            {
                return displayName;
            }

            byte[]? displayNameBuffer = _displayNameBuffer;
            if (displayNameBuffer is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            ReadOnlySpan<byte> displayNameBufferSpan = displayNameBuffer.AsSpan(0, _nameLength);
            displayName = StringPool.Shared.GetOrAdd(displayNameBufferSpan, Encoding.ASCII);

            ArrayPool<byte>.Shared.Return(displayNameBuffer);
            _displayNameBuffer = null;
            _displayName = displayName;
            return displayName;
        }
        init { }
    }

    public override required string Username
    {
        get
        {
            string? username = _username;
            if (username is not null)
            {
                return username;
            }

            byte[]? usernameBuffer = _usernameBuffer;

            string? displayName = _displayName;
            if (displayName is not null)
            {
                username = displayName.ToLowerInvariant();
                ArrayPool<byte>.Shared.Return(usernameBuffer);
                _usernameBuffer = null;
                _username = username;
                return username;
            }

            if (usernameBuffer is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            ReadOnlySpan<byte> usernameBufferSpan = usernameBuffer.AsSpan(0, _nameLength);
            username = StringPool.Shared.GetOrAdd(usernameBufferSpan, Encoding.ASCII);

            ArrayPool<byte>.Shared.Return(usernameBuffer);
            _usernameBuffer = null;
            _username = username;
            return username;
        }
        init { }
    }

    private Badge[]? _badgeInfos;
    private readonly int _badgeInfoCount;

    private Badge[]? _badges;
    private readonly int _badgeCount;

    private string? _displayName;
    private string? _username;

    private byte[]? _displayNameBuffer;
    internal byte[]? _usernameBuffer;
    internal readonly int _nameLength;

    /// <summary>
    /// The default constructor of <see cref="MemoryEfficientChatMessage"/>.
    /// </summary>
    /// <param name="badgeInfos">The badge info buffer.</param>
    /// <param name="badgeInfoCount">The amount of written elements in the badge info buffer.</param>
    /// <param name="badges">The badge buffer.</param>
    /// <param name="badgeCount">The amount of written elements in the badge buffer.</param>
    /// <param name="flags">The chat message flags.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="username">The user's username buffer.</param>
    /// <param name="nameLength">The length of the user's username.</param>
    public MemoryEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageFlags flags,
        byte[] displayName, byte[] username, int nameLength)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _flags = flags;

        _displayNameBuffer = displayName;
        _usernameBuffer = username;
        _nameLength = nameLength;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ArrayPool<Badge>.Shared.Return(_badgeInfos);
        _badgeInfos = null;

        ArrayPool<Badge>.Shared.Return(_badges);
        _badges = null;

        ArrayPool<byte>.Shared.Return(_displayNameBuffer);
        _displayNameBuffer = null;

        ArrayPool<byte>.Shared.Return(_usernameBuffer);
        _usernameBuffer = null;

        Message.Dispose();
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] MemoryEfficientChatMessage? other) =>
        ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is MemoryEfficientChatMessage other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Id, TmiSentTs);

    public static bool operator ==(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => Equals(left, right);

    public static bool operator !=(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => !(left == right);
}
