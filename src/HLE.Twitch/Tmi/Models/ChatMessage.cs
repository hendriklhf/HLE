using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Text;

namespace HLE.Twitch.Tmi.Models;

[DebuggerDisplay("{ToString()}")]
public abstract class ChatMessage : IChatMessage, IEquatable<ChatMessage>
{
    public abstract ReadOnlySpan<Badge> BadgeInfos { get; }

    public abstract ReadOnlySpan<Badge> Badges { get; }

    public required Color Color { get; init; }

    public required LazyString DisplayName { get; init; }

    public bool IsFirstMessage => (_flags & ChatMessageFlags.IsFirstMessage) != 0;

    public required Guid Id { get; init; }

    public bool IsModerator => (_flags & ChatMessageFlags.IsModerator) != 0;

    public required long ChannelId { get; init; }

    public bool IsSubscriber => (_flags & ChatMessageFlags.IsSubscriber) != 0;

    public required long TmiSentTs { get; init; }

    public bool IsTurboUser => (_flags & ChatMessageFlags.IsTurboUser) != 0;

    public required long UserId { get; init; }

    public bool IsAction => (_flags & ChatMessageFlags.IsAction) != 0;

    public required LazyString Username { get; init; }

    public required string Channel { get; init; }

    public required LazyString Message { get; init; }

    private protected ChatMessageFlags _flags;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Message.Dispose();
        Username.Dispose();
        DisplayName.Dispose();
    }

    /// <summary>
    /// Returns the message in the following format: "&lt;#Channel&gt; Username: Message".
    /// </summary>
    /// <returns>The message in a readable format.</returns>
    [Pure]
    [SkipLocalsInit]
    public sealed override string ToString()
    {
        using ValueStringBuilder builder = new(stackalloc char[Channel.Length + Username.Length + Message.Length + 6]);
        builder.Append("<#", Channel, "> ", Username.AsSpan(), ": ", Message.AsSpan());
        return builder.ToString();
    }

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        int requiredDestinationSize = Channel.Length + Username.Length + Message.Length + "<#".Length + "> ".Length + ": ".Length;
        if (destination.Length < requiredDestinationSize)
        {
            charsWritten = 0;
            return false;
        }

        UnsafeBufferWriter<char> writer = new(destination);
        writer.Write("<#");
        writer.Write(Channel);
        writer.Write("> ");
        writer.Write(Username.AsSpan());
        writer.Write(": ");
        writer.Write(Message.AsSpan());

        charsWritten = writer.Count;
        return true;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] ChatMessage? other) => ReferenceEquals(this, other);

    [Pure]
    public bool Equals([NotNullWhen(true)] IChatMessage? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public sealed override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(ChatMessage? left, ChatMessage? right) => Equals(left, right);

    public static bool operator !=(ChatMessage? left, ChatMessage? right) => !(left == right);
}
