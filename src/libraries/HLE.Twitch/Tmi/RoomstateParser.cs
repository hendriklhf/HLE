using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Collections;
using HLE.Memory;
using HLE.Numerics;
using HLE.Text;

namespace HLE.Twitch.Tmi;

public sealed class RoomstateParser : IRoomstateParser, IEquatable<RoomstateParser>
{
    private static ReadOnlySpan<byte> EmoteOnlyTag => "emote-only"u8;

    private static ReadOnlySpan<byte> FollowersOnlyTag => "followers-only"u8;

    private static ReadOnlySpan<byte> R9KTag => "r9k"u8;

    private static ReadOnlySpan<byte> RoomIdTag => "room-id"u8;

    private static ReadOnlySpan<byte> SlowModeTag => "slow"u8;

    private static ReadOnlySpan<byte> SubsOnlyTag => "subs-only"u8;

    [Pure]
    [SkipLocalsInit]
    public void Parse(ReadOnlySpan<byte> ircMessage, out Roomstate roomstate)
    {
        int whitespaceCount;
        if (!MemoryHelpers.UseStackalloc<int>(ircMessage.Length))
        {
            int[] indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.Rent(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespacesBuffer.AsSpan());
            Parse(ircMessage, indicesOfWhitespacesBuffer.AsSpanUnsafe(..whitespaceCount), out roomstate);
            ArrayPool<int>.Shared.Return(indicesOfWhitespacesBuffer);
            return;
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespaces);
        Parse(ircMessage, indicesOfWhitespaces[..whitespaceCount], out roomstate);
    }

    [Pure]
    public void Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, out Roomstate roomstate)
    {
        ChangedRoomStates changedStates = 0;
        bool emoteOnly = false;
        int followersOnly = -1;
        bool r9K = false;
        long channelId = 0;
        int slowMode = 0;
        bool subsOnly = false;

        ReadOnlySpan<byte> tags = ircMessage[1..indicesOfWhitespaces[0]];
        int equalsSignIndex = tags.IndexOf((byte)'=');
        while (equalsSignIndex >= 0)
        {
            int semicolonIndex = tags.IndexOf((byte)';');
            ReadOnlySpan<byte> key = tags[..equalsSignIndex];
            // semicolonIndex is -1 if no semicolon has been found, reinterpreting -1 as Index returns ^0
            ReadOnlySpan<byte> value = tags[(equalsSignIndex + 1)..Unsafe.BitCast<int, Index>(semicolonIndex)];
            tags = semicolonIndex < 0 ? [] : tags[(semicolonIndex + 1)..];
            equalsSignIndex = tags.IndexOf((byte)'=');

            switch (key[0])
            {
                case (byte)'e' when key.SequenceEqual(EmoteOnlyTag):
                    emoteOnly = GetEmoteOnly(value);
                    changedStates |= ChangedRoomStates.EmoteOnly;
                    break;
                case (byte)'f' when key.SequenceEqual(FollowersOnlyTag):
                    followersOnly = GetFollowersOnly(value);
                    changedStates |= ChangedRoomStates.FollowersOnly;
                    break;
                case (byte)'r' when key.SequenceEqual(R9KTag):
                    r9K = GetR9K(value);
                    changedStates |= ChangedRoomStates.R9K;
                    break;
                case (byte)'r' when key.SequenceEqual(RoomIdTag):
                    channelId = GetChannelId(value);
                    break;
                case (byte)'s' when key.SequenceEqual(SlowModeTag):
                    slowMode = GetSlowMode(value);
                    changedStates |= ChangedRoomStates.SlowMode;
                    break;
                case (byte)'s' when key.SequenceEqual(SubsOnlyTag):
                    subsOnly = GetSubsOnly(value);
                    changedStates |= ChangedRoomStates.SubsOnly;
                    break;
            }
        }

        ReadOnlySpan<byte> channel = GetChannel(ircMessage, indicesOfWhitespaces);

        roomstate = new(changedStates)
        {
            Channel = StringPool.Shared.GetOrAdd(channel, Encoding.UTF8),
            ChannelId = channelId,
            EmoteOnly = emoteOnly,
            R9K = r9K,
            FollowersOnly = followersOnly,
            SlowMode = slowMode,
            SubsOnly = subsOnly
        };
    }

    private static ReadOnlySpan<byte> GetChannel(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => ircMessage[(indicesOfWhitespaces[^1] + 2)..];

    private static bool GetEmoteOnly(ReadOnlySpan<byte> value) => value[0] == '1';

    private static int GetFollowersOnly(ReadOnlySpan<byte> value)
        => value[0] == '-' ? -1 : NumberHelpers.ParsePositiveNumber<int>(value);

    private static bool GetR9K(ReadOnlySpan<byte> value) => value[0] == '1';

    private static long GetChannelId(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<long>(value);

    private static int GetSlowMode(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<int>(value);

    private static bool GetSubsOnly(ReadOnlySpan<byte> value) => value[0] == '1';

    [Pure]
    public bool Equals([NotNullWhen(true)] RoomstateParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RoomstateParser? left, RoomstateParser? right) => Equals(left, right);

    public static bool operator !=(RoomstateParser? left, RoomstateParser? right) => !(left == right);
}
