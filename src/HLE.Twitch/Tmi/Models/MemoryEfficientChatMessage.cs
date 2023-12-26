using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Twitch.Tmi.Models;

public sealed class MemoryEfficientChatMessage : ChatMessage, IDisposable, IEquatable<MemoryEfficientChatMessage>
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
        [MethodImpl(MethodImplOptions.Synchronized)]
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
            displayName = StringPool.Shared.GetOrAdd(displayNameBufferSpan, Encoding.UTF8);

            ArrayPool<byte>.Shared.Return(displayNameBuffer);
            _displayNameBuffer = null;
            _displayName = displayName;
            return displayName;
        }
        init { }
    }

    public override required string Username
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            string? username = _username;
            if (username is not null)
            {
                return username;
            }

            string? displayName = _displayName;
            if (displayName is not null)
            {
                username = displayName.ToLowerInvariant();
                ArrayPool<byte>.Shared.Return(_usernameBuffer);
                _usernameBuffer = null;
                _username = username;
                return username;
            }

            byte[]? usernameBuffer = _usernameBuffer;
            if (usernameBuffer is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            ReadOnlySpan<byte> usernameBufferSpan = usernameBuffer.AsSpan(0, _nameLength);
            username = StringPool.Shared.GetOrAdd(usernameBufferSpan, Encoding.UTF8);

            ArrayPool<byte>.Shared.Return(usernameBuffer);
            _usernameBuffer = null;
            _username = username;
            return username;
        }
        init { }
    }

    public override required string Message
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            string? message = _message;
            if (message is not null)
            {
                return message;
            }

            byte[]? messageBuffer = _messageBuffer;
            if (messageBuffer is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            Encoding utf8 = Encoding.UTF8;
            ReadOnlySpan<byte> messageBufferSpan = messageBuffer.AsSpan(0, _messageLength);
            message = messageBufferSpan.Length <= MaxMessagePoolingLength
                ? StringPool.Shared.GetOrAdd(messageBufferSpan, utf8)
                : utf8.GetString(messageBufferSpan);

            ArrayPool<byte>.Shared.Return(messageBuffer);
            _messageBuffer = null;
            _message = message;
            return message;
        }
        init { }
    }

    private Badge[]? _badgeInfos;
    private readonly int _badgeInfoCount;

    private Badge[]? _badges;
    private readonly int _badgeCount;

    private string? _displayName;
    private string? _username;
    private string? _message;

    private byte[]? _displayNameBuffer;
    private byte[]? _usernameBuffer;
    private readonly int _nameLength;

    private byte[]? _messageBuffer;
    private readonly int _messageLength;

    private const int MaxMessagePoolingLength = 10;

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
    /// <param name="message">The message buffer.</param>
    /// <param name="messageLength">The length of the message.</param>
    public MemoryEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageFlags flags,
        byte[] displayName, byte[] username, int nameLength, byte[] message, int messageLength)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _flags = flags;

        _displayNameBuffer = displayName;
        _usernameBuffer = username;
        _nameLength = nameLength;

        _messageBuffer = message;
        _messageLength = messageLength;
    }

    public void Dispose()
    {
        ArrayPool<Badge>.Shared.Return(_badgeInfos);
        _badgeInfos = null;

        ArrayPool<Badge>.Shared.Return(_badges);
        _badges = null;

        ArrayPool<byte>.Shared.Return(_displayNameBuffer);
        _displayNameBuffer = null;

        ArrayPool<byte>.Shared.Return(_usernameBuffer);
        _usernameBuffer = null;

        ArrayPool<byte>.Shared.Return(_messageBuffer);
        _messageBuffer = null;
    }

    [Pure]
    [SkipLocalsInit]
    public override string ToString()
    {
        ValueStringBuilder builder = new(stackalloc char[Channel.Length + _nameLength + _messageLength + 6]);
        builder.Append("<#", Channel, "> ", Username, ": ", Message);
        return builder.ToString();
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
