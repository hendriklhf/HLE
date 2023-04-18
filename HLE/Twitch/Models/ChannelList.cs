using System;
using System.Collections;
using System.Collections.Generic;
using HLE.Collections;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a list of channels the client is connected to.
/// </summary>
public sealed class ChannelList : IEnumerable<Channel>
{
    /// <summary>
    /// Retrieves a channel by the user id of the channel owner. Returns null if the client is not connected to the channel.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner.</param>
    public Channel? this[long channelId] => Get(channelId);

    /// <inheritdoc cref="this[ReadOnlySpan{char}]"/>
    public Channel? this[string channelName] => Get(channelName);

    /// <inheritdoc cref="this[ReadOnlySpan{char}]"/>
    public Channel? this[ReadOnlyMemory<char> channelName] => Get(channelName.Span);

    /// <summary>
    /// Retrieves a channel by the username of the channel owner. Returns null if the client is not connected to channel.
    /// </summary>
    /// <param name="channelName">The channel name, with or without '#'.</param>
    public Channel? this[ReadOnlySpan<char> channelName] => Get(channelName);

    public int Count => _channels.Count;

    private readonly DoubleDictionary<long, int, Channel> _channels = new();

    internal void Update(in RoomstateArgs args)
    {
        Channel? channel = Get(args.ChannelId);
        if (channel is null)
        {
            channel = new(in args);
            int channelNameHash = string.GetHashCode(channel.Name, StringComparison.OrdinalIgnoreCase);
            _channels.Add(channel.Id, channelNameHash, channel);
        }
        else
        {
            channel.Update(in args);
        }
    }

    internal void Remove(ReadOnlySpan<char> channelName)
    {
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
        _channels.Clear();
    }

    private Channel? Get(long channelId)
    {
        return _channels.TryGetValue(channelId, out Channel? channel) ? channel : null;
    }

    private Channel? Get(ReadOnlySpan<char> name)
    {
        if (name[0] == '#')
        {
            name = name[1..];
        }

        int channelNameHash = string.GetHashCode(name, StringComparison.OrdinalIgnoreCase);
        return _channels.TryGetValue(channelNameHash, out Channel? channel) ? channel : null;
    }

    public bool Equals(ChannelList? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return _channels.GetHashCode();
    }

    public IEnumerator<Channel> GetEnumerator()
    {
        return _channels.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
