using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;
using HLE.Text;

namespace HLE.Twitch.Tmi.Models;

/// <summary>
/// The default constructor of <see cref="ChatMessage"/>.
/// </summary>
/// <param name="badgeInfos">The badge info buffer.</param>
/// <param name="badgeInfoCount">The amount of written elements in the badge info buffer.</param>
/// <param name="badges">The badge buffer.</param>
/// <param name="badgeCount">The amount of written elements in the badge buffer.</param>
/// <param name="flags">The chat message flags.</param>
public sealed class ChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageFlags flags) :
    IDisposable,
    ISpanFormattable,
    IEquatable<ChatMessage>
{
    public ReadOnlySpan<Badge> BadgeInfos
    {
        get
        {
            Badge[]? badgeInfos = _badgeInfos;
            if (badgeInfos is null)
            {
                ThrowHelper.ThrowObjectDisposedException<ChatMessage>();
            }

            return badgeInfos.AsSpanUnsafe(.._badgeInfoCount);
        }
    }

    public ReadOnlySpan<Badge> Badges
    {
        get
        {
            Badge[]? badges = _badges;
            if (badges is null)
            {
                ThrowHelper.ThrowObjectDisposedException<ChatMessage>();
            }

            return badges.AsSpanUnsafe(.._badgeCount);
        }
    }

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

    private readonly ChatMessageFlags _flags = flags;

    private Badge[]? _badgeInfos = badgeInfos;
    private readonly int _badgeInfoCount = badgeInfoCount;

    private Badge[]? _badges = badges;
    private readonly int _badgeCount = badgeCount;

    public void Dispose()
    {
        Badge[]? badgeInfos = Interlocked.Exchange(ref _badgeInfos, null);
        if (badgeInfos is not null)
        {
            ArrayPool<Badge>.Shared.Return(badgeInfos);
        }

        Badge[]? badges = Interlocked.Exchange(ref _badges, null);
        if (badges is not null)
        {
            ArrayPool<Badge>.Shared.Return(badges);
        }

        Username.DisposeInterlocked();
        DisplayName.DisposeInterlocked();
        Message.DisposeInterlocked();
    }

    /// <summary>
    /// Returns the message in the following format: "&lt;#Channel&gt; Username: Message".
    /// </summary>
    /// <returns>The message in a readable format.</returns>
    [Pure]
    [SkipLocalsInit]
    public override string ToString()
    {
        Span<char> buffer = stackalloc char[Channel.Length + Username.Length + Message.Length + "<#".Length + "> ".Length + ": ".Length];
        int length = Format(buffer);
        return new(buffer[..length]);
    }

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

    [SkipLocalsInit]
    public void Format(TextWriter writer)
    {
        Span<char> buffer = stackalloc char[Channel.Length + Username.Length + Message.Length + "<#".Length + "> ".Length + ": ".Length];
        int length = Format(buffer);
        writer.WriteLine(buffer[..length]);
    }

    public async Task FormatAsync(TextWriter writer)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(Channel.Length + Username.Length + Message.Length + "<#".Length + "> ".Length + ": ".Length);
        try
        {
            int length = Format(buffer);
            await writer.WriteLineAsync(buffer.AsMemory(..length));
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        int requiredDestinationSize = Channel.Length + Username.Length + Message.Length + "<#".Length + "> ".Length + ": ".Length;
        if (destination.Length < requiredDestinationSize)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = Format(destination);
        return true;
    }

    private int Format(Span<char> destination)
    {
        Debug.Assert(destination.Length >= Channel.Length + Username.Length + Message.Length + "<#".Length + "> ".Length + ": ".Length);

        UnsafeBufferWriter<char> writer = new(destination);
        writer.Write("<#");
        writer.Write(Channel);
        writer.Write("> ");
        writer.Write(Username.AsSpan());
        writer.Write(": ");
        writer.Write(Message.AsSpan());
        return writer.Count;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] ChatMessage? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ChatMessage? left, ChatMessage? right) => Equals(left, right);

    public static bool operator !=(ChatMessage? left, ChatMessage? right) => !(left == right);
}
