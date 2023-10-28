using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Collections.Concurrent;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a list of channels the client is connected to.
/// </summary>
[DebuggerDisplay("Count = {_channels.Count}")]
public sealed class ChannelList : IReadOnlyCollection<Channel>, IEquatable<ChannelList>, ICountable, IDisposable, ICollectionProvider<Channel>
{
    /// <summary>
    /// Gets the amount of channels in the list.
    /// </summary>
    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));
            return _channels.Count;
        }
    }

    /// <summary>
    /// Uses channel id as primary key and hashed channel name with OrdinalIgnoreCase comparison as secondary key.
    /// </summary>
    private ConcurrentDoubleDictionary<long, int, Channel>? _channels = new();

    /// <summary>
    /// Retrieves a channel by the user id of the channel owner.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner.</param>
    /// <param name="channel">The channel.</param>;
    public bool TryGet(long channelId, [MaybeNullWhen(false)] out Channel channel)
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));
        return _channels.TryGetByPrimaryKey(channelId, out channel);
    }

    /// <summary>
    /// Retrieves a channel by the username of the channel owner.
    /// </summary>
    /// <param name="channelName">The channel name, with or without '#'.</param>
    /// <param name="channel">The channel.</param>
    public bool TryGet(ReadOnlySpan<char> channelName, [MaybeNullWhen(false)] out Channel channel)
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));

        if (channelName.Length == 0)
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

    public void Dispose()
    {
        _channels?.Dispose();
        _channels = null;
    }

    internal void Update(in Roomstate args)
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));

        Channel? channel = Get(args.ChannelId);
        if (channel is not null)
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
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));

        Channel? channel = Get(channelName);
        if (channel is null)
        {
            return;
        }

        int channelNameHash = string.GetHashCode(channel.Name, StringComparison.OrdinalIgnoreCase);
        _channels.Remove(channel.Id, channelNameHash);
    }

    internal void Clear()
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));
        _channels.Clear();
    }

    private Channel? Get(long channelId)
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));
        return _channels.TryGetByPrimaryKey(channelId, out Channel? channel) ? channel : null;
    }

    private Channel? Get(ReadOnlySpan<char> name)
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));

        if (name.Length == 0)
        {
            return null;
        }

        if (name[0] == '#')
        {
            name = name[1..];
            if (name.Length == 0)
            {
                return null;
            }
        }

        int channelNameHash = string.GetHashCode(name, StringComparison.OrdinalIgnoreCase);
        return _channels.TryGetBySecondaryKey(channelNameHash, out Channel? channel) ? channel : null;
    }

    [Pure]
    public Channel[] ToArray()
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));
        return _channels.ToArray();
    }

    [Pure]
    public List<Channel> ToList()
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));
        return _channels.ToList();
    }

    [Pure]
    public bool Equals(ChannelList? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public IEnumerator<Channel> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(_channels is null, typeof(ChannelList));
        return _channels.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
