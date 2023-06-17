using System;
using System.Diagnostics.Contracts;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Twitch.Models;

public sealed class MemoryEfficientChatMessage : ChatMessage, IEquatable<MemoryEfficientChatMessage>
{
    public override ReadOnlySpan<Badge> BadgeInfos => _badgeInfos[.._badgeInfoCount];

    public override ReadOnlySpan<Badge> Badges => _badges[.._badgeCount];

    public override required string DisplayName
    {
        get => _displayName ??= _displayNamePool.GetOrAdd(_displayNameBuilder.WrittenSpan);
        init { }
    }

    public override required string Username
    {
        get => _username ??= _usernamePool.GetOrAdd(_usernameBuilder.WrittenSpan);
        init { }
    }

    public override required string Message
    {
        get => _message ??= _messageBuilder.Length <= _maxMessagePoolingLength ? _shortMessagesPool.GetOrAdd(_messageBuilder.WrittenSpan) : _messageBuilder.ToString();
        init { }
    }

    private readonly RentedArray<Badge> _badgeInfos;
    private readonly int _badgeInfoCount;
    private readonly RentedArray<Badge> _badges;
    private readonly int _badgeCount;

    private string? _displayName;
    private string? _username;
    private string? _message;

    private readonly PoolBufferStringBuilder _displayNameBuilder;
    private readonly PoolBufferStringBuilder _usernameBuilder;
    private readonly PoolBufferStringBuilder _messageBuilder;

    private static readonly StringPool _shortMessagesPool = new();
    private static readonly StringPool _usernamePool = new();
    private static readonly StringPool _displayNamePool = new();

    private const int _maxMessagePoolingLength = 25;

    /// <summary>
    /// The default constructor of <see cref="MemoryEfficientChatMessage"/>.
    /// </summary>
    public MemoryEfficientChatMessage(RentedArray<Badge> badgeInfos, int badgeInfoCount, RentedArray<Badge> badges, int badgeCount, ChatMessageTags tags,
        ReadOnlySpan<char> displayName, ReadOnlySpan<char> username, ReadOnlySpan<char> message)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _tags = tags;

        _displayNameBuilder = new(displayName.Length);
        _displayNameBuilder.Append(displayName);

        _usernameBuilder = new(username.Length);
        _usernameBuilder.Append(username);

        _messageBuilder = new(message.Length);
        _messageBuilder.Append(message);
    }

    ~MemoryEfficientChatMessage()
    {
        _badgeInfos.Dispose();
        _badges.Dispose();
        _displayNameBuilder.Dispose();
        _usernameBuilder.Dispose();
        _messageBuilder.Dispose();
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _badgeInfos.Dispose();
        _badges.Dispose();
        _displayNameBuilder.Dispose();
        _usernameBuilder.Dispose();
        _messageBuilder.Dispose();
    }

    [Pure]
    public override string ToString()
    {
        ValueStringBuilder builder = stackalloc char[Channel.Length + _usernameBuilder.Length + _messageBuilder.Length + 6];
        builder.Append("<#", Channel, "> ", _usernameBuilder.WrittenSpan, ": ", _messageBuilder.WrittenSpan);
        return builder.ToString();
    }

    public bool Equals(MemoryEfficientChatMessage? other)
    {
        return ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);
    }

    public override bool Equals(object? obj)
    {
        return obj is MemoryEfficientChatMessage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, TmiSentTs);
    }

    public static bool operator ==(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right)
    {
        return !(left == right);
    }
}
