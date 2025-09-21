using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Options for <see cref="TwitchClient"/>.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ClientOptions : IBitwiseEquatable<ClientOptions>
{
    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    public bool UseSsl
    {
        get => ReadFlag(_flags, Flags.UseSsl);
        init => WriteFlag(ref _flags, Flags.UseSsl, value);
    }

    /// <summary>
    /// Indicates whether the bot is verified or not. If your bot is verified, you can set this to true. Verified bots have higher rate limits.
    /// </summary>
    public bool IsVerifiedBot
    {
        get => ReadFlag(_flags, Flags.IsVerifiedBot);
        init => WriteFlag(ref _flags, Flags.IsVerifiedBot, value);
    }

    [SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "makes no sense")]
    public byte WebSocketProcessingThreadCount
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfZero(value);
            field = value;
        }
    }

    public bool ReceiveMembershipMessages
    {
        get => ReadFlag(_flags, Flags.ReceiveMembershipMessages);
        init => WriteFlag(ref _flags, Flags.ReceiveMembershipMessages, value);
    }

    public bool ReceiveRoomstateMessages
    {
        get => ReadFlag(_flags, Flags.ReceiveRoomstateMessages);
        init => WriteFlag(ref _flags, Flags.ReceiveRoomstateMessages, value);
    }

    public bool ReceiveChatMessages
    {
        get => ReadFlag(_flags, Flags.ReceiveChatMessages);
        init => WriteFlag(ref _flags, Flags.ReceiveChatMessages, value);
    }

    public bool ReceiveNoticeMessages
    {
        get => ReadFlag(_flags, Flags.ReceiveNoticeMessages);
        init => WriteFlag(ref _flags, Flags.ReceiveNoticeMessages, value);
    }

    private readonly Flags _flags;

    public static ClientOptions Default => new();

    private const byte DefaultWebSocketProcessingThreadCount = 2;

    public ClientOptions()
    {
        _flags = Flags.UseSsl | Flags.ReceiveMembershipMessages | Flags.ReceiveRoomstateMessages | Flags.ReceiveChatMessages | Flags.ReceiveNoticeMessages;
        WebSocketProcessingThreadCount = DefaultWebSocketProcessingThreadCount;
    }

    private static bool ReadFlag(Flags flags, [ConstantExpected] Flags flag) => (flags & flag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteFlag(ref Flags flags, [ConstantExpected] Flags flag, bool value)
    {
        if (value)
        {
            flags |= flag;
        }
        else
        {
            flags &= ~flag;
        }
    }

    [Pure]
    public bool Equals(ClientOptions other) => StructMarshal.EqualsBitwise(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ClientOptions other && Equals(other);

    [Pure]
    public override int GetHashCode()
    {
        ClientOptions t = this;
        Span<byte> bytes = StructMarshal.AsBytes(ref t);
        return (int)XxHash32.HashToUInt32(bytes);
    }

    public static bool operator ==(ClientOptions left, ClientOptions right) => left.Equals(right);

    public static bool operator !=(ClientOptions left, ClientOptions right) => !(left == right);
}
