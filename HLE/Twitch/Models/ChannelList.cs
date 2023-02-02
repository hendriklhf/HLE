using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    /// <inheritdoc cref="this[ReadOnlySpan{char}]"/>
    public Channel? this[string channel] => Get(channel);

    /// <summary>
    /// Retrieves a channel by the username of the channel owner. Returns null if the client is not connected to channel.
    /// </summary>
    /// <param name="channel">The channel name, with or without '#'.</param>
    public Channel? this[ReadOnlySpan<char> channel] => Get(channel);

    public int Count => _channels.Count;

    private readonly List<Channel> _channels = new();

    internal void Update(in RoomstateArgs args)
    {
        Channel? channel = Get(args.ChannelId);
        if (channel is null)
        {
            channel = new(in args);
            _channels.Add(channel);
        }
        else
        {
            channel.Update(in args);
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

    internal void Clear()
    {
        _channels.Clear();
    }

    private Channel? Get(long id)
    {
        ReadOnlySpan<Channel> channels = CollectionsMarshal.AsSpan(_channels);
        int channelsLength = channels.Length;
        ref Channel firstChannel = ref MemoryMarshal.GetReference(channels);
        for (int i = 0; i < channelsLength; i++)
        {
            Channel channel = Unsafe.Add(ref firstChannel, i);
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

        ReadOnlySpan<Channel> channels = CollectionsMarshal.AsSpan(_channels);
        int channelsLength = channels.Length;
        ref Channel firstChannel = ref MemoryMarshal.GetReference(channels);
        for (int i = 0; i < channelsLength; i++)
        {
            Channel channel = Unsafe.Add(ref firstChannel, i);
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
