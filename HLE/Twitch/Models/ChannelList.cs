using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

    /// <summary>
    /// Retrieves a channel by the username of the channel owner. Returns null if the client is not connected to channel.
    /// </summary>
    /// <param name="channel">The username of the channel owner.</param>
    public Channel? this[string channel] => Get(channel);

    private readonly List<Channel> _channels = new();

    internal void Update(RoomstateArgs args)
    {
        Channel? channel = Get(args.ChannelId);
        if (channel is null)
        {
            channel = new(args);
            _channels.Add(channel);
        }
        else
        {
            channel.Update(args);
        }
    }

    internal void Remove(ReadOnlySpan<char> name)
    {
        Channel? channel = Get(name);
        if (channel is not null)
        {
            _channels.Remove(channel);
        }
    }

    internal void Remove(long id)
    {
        Channel? channel = Get(id);
        if (channel is null)
        {
            return;
        }

        _channels.Remove(channel);
    }

    internal void Clear()
    {
        _channels.Clear();
    }

    private Channel? Get(long id)
    {
        ReadOnlySpan<Channel> channels = CollectionsMarshal.AsSpan(_channels);
        for (int i = 0; i < channels.Length; i++)
        {
            Channel channel = channels[i];
            if (channel.Id == id)
            {
                return channel;
            }
        }

        return null;
    }

    private Channel? Get(ReadOnlySpan<char> name)
    {
        if (name[0] == '#')
        {
            name = name[1..];
        }

        ReadOnlySpan<Channel> channelSpan = CollectionsMarshal.AsSpan(_channels);
        for (int i = 0; i < channelSpan.Length; i++)
        {
            Channel channel = channelSpan[i];
            if (name.Equals(channel.Name, StringComparison.OrdinalIgnoreCase))
            {
                return channel;
            }
        }

        return null;
    }

    public IEnumerator<Channel> GetEnumerator()
    {
        return _channels.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
