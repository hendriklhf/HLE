using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HLE.Twitch.Args;
using HLE.Twitch.Models;

namespace HLE.Twitch;

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
        Channel? channel = this.FirstOrDefault(c => c.Id == args.ChannelId);
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

    internal void Remove(string name)
    {
        Channel? channel = this.FirstOrDefault(c => string.Equals(name, c.Name, StringComparison.OrdinalIgnoreCase));
        if (channel is null)
        {
            return;
        }

        _channels.Remove(channel);
    }

    internal void Remove(long id)
    {
        Channel? channel = this.FirstOrDefault(c => id == c.Id);
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
        return this.FirstOrDefault(c => c.Id == id);
    }

    private Channel? Get(string name)
    {
        return this.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
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
