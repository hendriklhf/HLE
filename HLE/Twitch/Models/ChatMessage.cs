using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using HLE.Strings;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public abstract class ChatMessage : IChatMessage, IEquatable<ChatMessage>
{
    public abstract ReadOnlySpan<Badge> BadgeInfos { get; }

    public abstract ReadOnlySpan<Badge> Badges { get; }

    public required Color Color { get; init; }

    public virtual required string DisplayName { get; init; }

    public bool IsFirstMessage => (_flags & ChatMessageFlag.IsFirstMessage) == ChatMessageFlag.IsFirstMessage;

    public required Guid Id { get; init; }

    public bool IsModerator => (_flags & ChatMessageFlag.IsModerator) == ChatMessageFlag.IsModerator;

    public required long ChannelId { get; init; }

    public bool IsSubscriber => (_flags & ChatMessageFlag.IsSubscriber) == ChatMessageFlag.IsSubscriber;

    public required long TmiSentTs { get; init; }

    public bool IsTurboUser => (_flags & ChatMessageFlag.IsTurboUser) == ChatMessageFlag.IsTurboUser;

    public required long UserId { get; init; }

    public bool IsAction => (_flags & ChatMessageFlag.IsAction) == ChatMessageFlag.IsAction;

    public virtual required string Username { get; init; }

    public required string Channel { get; init; }

    public virtual required string Message { get; init; }

    private protected ChatMessageFlag _flags;

    public abstract void Dispose();

    /// <summary>
    /// Returns the message in the following format: "&lt;#Channel&gt; Username: Message".
    /// </summary>
    /// <returns>The message in a readable format.</returns>
    [Pure]
    public override string ToString()
    {
        ValueStringBuilder builder = stackalloc char[Channel.Length + Username.Length + Message.Length + 6];
        builder.Append("<#", Channel, "> ", Username, ": ", Message);
        return builder.ToString();
    }

    [Pure]
    public bool Equals(ChatMessage? other)
    {
        return Equals((IChatMessage?)other);
    }

    [Pure]
    public bool Equals(IChatMessage? other)
    {
        return ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is IChatMessage other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, TmiSentTs);
    }

    public static bool operator ==(ChatMessage? left, ChatMessage? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ChatMessage? left, ChatMessage? right)
    {
        return !(left == right);
    }
}
