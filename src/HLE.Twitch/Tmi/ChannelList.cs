using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Collections.Concurrent;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

/// <summary>
/// A class that represents a list of channels the client is connected to.
/// </summary>
[DebuggerDisplay("Count = {_channels.Count}")]
public sealed class ChannelList : IReadOnlyCollection<Channel>, IEquatable<ChannelList>, ICountable, ICollectionProvider<Channel>
{
    /// <summary>
    /// Gets the amount of channels in the list.
    /// </summary>
    public int Count => _channels.Count;

    /// <summary>
    /// Uses channel id as primary key and hashed channel name with OrdinalIgnoreCase comparison as secondary key.
    /// </summary>
    private readonly ConcurrentDoubleDictionary<long, int, Channel> _channels = new();

    /// <summary>
    /// Retrieves a channel by the user id of the channel owner.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner.</param>
    /// <param name="channel">The channel.</param>;
    public bool TryGet(long channelId, [MaybeNullWhen(false)] out Channel channel)
        => _channels.TryGetByPrimaryKey(channelId, out channel);

    /// <summary>
    /// Retrieves a channel by the username of the channel owner.
    /// </summary>
    /// <param name="channelName">The channel name, with or without '#'.</param>
    /// <param name="channel">The channel.</param>
    public bool TryGet(ReadOnlySpan<char> channelName, [MaybeNullWhen(false)] out Channel channel)
    {
        if (channelName.Length is < ChannelFormatter.MinimumChannelNameLength or > ChannelFormatter.MaximumPrefixedChannelNameLength)
        {
            channel = null;
            return false;
        }

        if (channelName[0] == '#')
        {
            channelName = channelName[1..];
            if (channelName.Length == 0)
            {
                channel = null;
                return false;
            }
        }

        int channelNameHash = string.GetHashCode(channelName, StringComparison.OrdinalIgnoreCase);
        return _channels.TryGetBySecondaryKey(channelNameHash, out channel);
    }

    internal void Update(in Roomstate args)
    {
        if (TryGet(args.ChannelId, out Channel? channel))
        {
            channel.Update(in args);
            return;
        }

        channel = new(in args);
        int channelNameHash = string.GetHashCode(channel.Name, StringComparison.OrdinalIgnoreCase);
        _channels.AddOrSet(channel.Id, channelNameHash, channel);
    }

    internal void Remove(ReadOnlySpan<char> channelName)
    {
        if (!TryGet(channelName, out Channel? channel))
        {
            return;
        }

        int channelNameHash = string.GetHashCode(channel.Name, StringComparison.OrdinalIgnoreCase);
        _channels.Remove(channel.Id, channelNameHash);
    }

    internal void Clear() => _channels.Clear();

    [Pure]
    public Channel[] ToArray() => _channels.ToArray();

    [Pure]
    public Channel[] ToArray(int start) => _channels.ToArray(start..);

    [Pure]
    public Channel[] ToArray(int start, int length) => _channels.ToArray(start, length);

    [Pure]
    public Channel[] ToArray(Range range) => _channels.ToArray(range);

    [Pure]
    public List<Channel> ToList() => _channels.ToList();

    [Pure]
    public List<Channel> ToList(int start) => _channels.ToList(start..);

    [Pure]
    public List<Channel> ToList(int start, int length) => _channels.ToList(start, length);

    [Pure]
    public List<Channel> ToList(Range range) => _channels.ToList(range);

    [Pure]
    public bool Equals([NotNullWhen(true)] ChannelList? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public IEnumerator<Channel> GetEnumerator() => Count == 0 ? EmptyEnumeratorCache<Channel>.Enumerator : _channels.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
