using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Memory;
using HLE.Numerics;
using HLE.Strings;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public sealed class RoomstateParser : IRoomstateParser, IEquatable<RoomstateParser>
{
    private const string _emoteOnlyTag = "emote-only";
    private const string _followersOnlyTag = "followers-only";
    private const string _r9KTag = "r9k";
    private const string _roomIdTag = "room-id";
    private const string _slowModeTag = "slow";
    private const string _subsOnlyTag = "subs-only";

    [Pure]
    [SkipLocalsInit]
    public void Parse(ReadOnlySpan<char> ircMessage, out Roomstate roomstate)
    {
        int whitespaceCount;
        if (!MemoryHelper.UseStackAlloc<int>(ircMessage.Length))
        {
            using RentedArray<int> indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespacesBuffer.AsSpan());
            Parse(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount], out roomstate);
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespaces);
        Parse(ircMessage, indicesOfWhitespaces[..whitespaceCount], out roomstate);
    }

    [Pure]
    public void Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, out Roomstate roomstate)
    {
        ChangedRoomStates changedStates = 0;
        bool emoteOnly = false;
        int followersOnly = -1;
        bool r9K = false;
        long channelId = 0;
        int slowMode = 0;
        bool subsOnly = false;

        ReadOnlySpan<char> tags = ircMessage[1..indicesOfWhitespaces[0]];
        int equalsSignIndex = tags.IndexOf('=');
        while (equalsSignIndex >= 0)
        {
            int semicolonIndex = tags.IndexOf(';');
            ReadOnlySpan<char> key = tags[..equalsSignIndex];
            // semicolonIndex is -1 if no semicolon has been found, reinterpreting -1 as Index returns ^0
            ReadOnlySpan<char> value = tags[(equalsSignIndex + 1)..Unsafe.As<int, Index>(ref semicolonIndex)];
            tags = semicolonIndex < 0 ? [] : tags[(semicolonIndex + 1)..];
            equalsSignIndex = tags.IndexOf('=');

            switch (key)
            {
                case _emoteOnlyTag:
                    emoteOnly = GetEmoteOnly(value);
                    changedStates |= ChangedRoomStates.EmoteOnly;
                    break;
                case _followersOnlyTag:
                    followersOnly = GetFollowersOnly(value);
                    changedStates |= ChangedRoomStates.FollowersOnly;
                    break;
                case _r9KTag:
                    r9K = GetR9K(value);
                    changedStates |= ChangedRoomStates.R9K;
                    break;
                case _roomIdTag:
                    channelId = GetChannelId(value);
                    break;
                case _slowModeTag:
                    slowMode = GetSlowMode(value);
                    changedStates |= ChangedRoomStates.SlowMode;
                    break;
                case _subsOnlyTag:
                    subsOnly = GetSubsOnly(value);
                    changedStates |= ChangedRoomStates.SubsOnly;
                    break;
            }
        }

        ReadOnlySpan<char> channel = GetChannel(ircMessage, indicesOfWhitespaces);

        roomstate = new(changedStates)
        {
            Channel = StringPool.Shared.GetOrAdd(channel),
            ChannelId = channelId,
            EmoteOnly = emoteOnly,
            R9K = r9K,
            FollowersOnly = followersOnly,
            SlowMode = slowMode,
            SubsOnly = subsOnly
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> GetChannel(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => ircMessage[(indicesOfWhitespaces[^1] + 2)..];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetEmoteOnly(ReadOnlySpan<char> value) => value[0] == '1';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFollowersOnly(ReadOnlySpan<char> value)
        => value[0] == '-' ? -1 : NumberHelper.ParsePositiveNumber<int>(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetR9K(ReadOnlySpan<char> value) => value[0] == '1';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetChannelId(ReadOnlySpan<char> value) => NumberHelper.ParsePositiveNumber<long>(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetSlowMode(ReadOnlySpan<char> value) => NumberHelper.ParsePositiveNumber<int>(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetSubsOnly(ReadOnlySpan<char> value) => value[0] == '1';

    [Pure]
    public bool Equals(RoomstateParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RoomstateParser? left, RoomstateParser? right) => Equals(left, right);

    public static bool operator !=(RoomstateParser? left, RoomstateParser? right) => !(left == right);
}
