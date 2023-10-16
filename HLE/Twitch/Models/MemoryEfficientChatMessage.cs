using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Twitch.Models;

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
        get
        {
            if (_displayName is not null)
            {
                return _displayName;
            }

            ObjectDisposedException.ThrowIf(_displayNameBuffer is null, typeof(MemoryEfficientChatMessage));

            _displayName = StringPool.Shared.GetOrAdd(_displayNameBuffer.AsSpan(.._nameLength));
            ArrayPool<char>.Shared.Return(_displayNameBuffer!);
            _displayNameBuffer = null;
            return _displayName;
        }
        init { }
    }

    public override required string Username
    {
        get
        {
            if (_username is not null)
            {
                return _username;
            }

            ObjectDisposedException.ThrowIf(_usernameBuffer is null, typeof(MemoryEfficientChatMessage));

            _username = StringPool.Shared.GetOrAdd(_usernameBuffer.AsSpan(.._nameLength));
            ArrayPool<char>.Shared.Return(_usernameBuffer!);
            _usernameBuffer = null;
            return _username;
        }
        init { }
    }

    public override required string Message
    {
        get
        {
            if (_message is not null)
            {
                return _message;
            }

            ObjectDisposedException.ThrowIf(_messageBuffer is null, typeof(MemoryEfficientChatMessage));

            ReadOnlySpan<char> message = _messageBuffer.AsSpan(.._messageLength);
            _message = message.Length <= _maxMessagePoolingLength ? StringPool.Shared.GetOrAdd(message) : new(message);
            ArrayPool<char>.Shared.Return(_messageBuffer!);
            _messageBuffer = null;
            return _message;
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

    private char[]? _displayNameBuffer;
    private char[]? _usernameBuffer;
    private readonly int _nameLength;

    private char[]? _messageBuffer;
    private readonly int _messageLength;

    private const int _maxMessagePoolingLength = 25;

    /// <summary>
    /// The default constructor of <see cref="MemoryEfficientChatMessage"/>.
    /// </summary>
    public MemoryEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageTags tags,
        char[] displayName, char[] username, int nameLength, char[] message, int messageLength)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _tags = tags;

        _displayNameBuffer = displayName;
        _usernameBuffer = username;
        _nameLength = nameLength;

        _messageBuffer = message;
        _messageLength = messageLength;
    }

    public void Dispose()
    {
        if (_badgeInfos is not null)
        {
            ArrayPool<Badge>.Shared.Return(_badgeInfos);
            _badgeInfos = null;
        }

        if (_badges is not null)
        {
            ArrayPool<Badge>.Shared.Return(_badges);
            _badges = null;
        }

        if (_displayNameBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_displayNameBuffer);
            _displayNameBuffer = null;
        }

        if (_usernameBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_usernameBuffer);
            _usernameBuffer = null;
        }

        if (_messageBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_messageBuffer);
            _messageBuffer = null;
        }
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
    public bool Equals(MemoryEfficientChatMessage? other) => ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);

    [Pure]
    public override bool Equals(object? obj) => obj is MemoryEfficientChatMessage other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Id, TmiSentTs);

    public static bool operator ==(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => Equals(left, right);

    public static bool operator !=(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => !(left == right);
}
