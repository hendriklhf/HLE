using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Strings;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public abstract class ChatMessage : IChatMessage, IEquatable<ChatMessage>
{
    public abstract ReadOnlySpan<Badge> BadgeInfos { get; }

    public abstract ReadOnlySpan<Badge> Badges { get; }

    public required Color Color { get; init; }

    public virtual required string DisplayName { get; init; }

    public bool IsFirstMessage => (_flags & ChatMessageFlags.IsFirstMessage) != 0;

    public required Guid Id { get; init; }

    public bool IsModerator => (_flags & ChatMessageFlags.IsModerator) != 0;

    public required long ChannelId { get; init; }

    public bool IsSubscriber => (_flags & ChatMessageFlags.IsSubscriber) != 0;

    public required long TmiSentTs { get; init; }

    public bool IsTurboUser => (_flags & ChatMessageFlags.IsTurboUser) != 0;

    public required long UserId { get; init; }

    public bool IsAction => (_flags & ChatMessageFlags.IsAction) != 0;

    public virtual required string Username { get; init; }

    public required string Channel { get; init; }

    public virtual required string Message { get; init; }

    private protected ChatMessageFlags _flags;

    /// <summary>
    /// Returns the message in the following format: "&lt;#Channel&gt; Username: Message".
    /// </summary>
    /// <returns>The message in a readable format.</returns>
    [Pure]
    [SkipLocalsInit]
    public override string ToString()
    {
        ValueStringBuilder builder = new(stackalloc char[Channel.Length + Username.Length + Message.Length + 6]);
        builder.Append("<#", Channel, "> ", Username, ": ", Message);
        return builder.ToString();
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] ChatMessage? other) => Equals((IChatMessage?)other);

    [Pure]
    public bool Equals([NotNullWhen(true)] IChatMessage? other) => ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is IChatMessage other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Id, TmiSentTs);

    public static bool operator ==(ChatMessage? left, ChatMessage? right) => Equals(left, right);

    public static bool operator !=(ChatMessage? left, ChatMessage? right) => !(left == right);
}
