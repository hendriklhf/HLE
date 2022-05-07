using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HLE.Twitch.Args;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public class ChannelList : IEnumerable<Channel>
{
    public Channel? this[long channelId] => Get(channelId);

    public Channel? this[string channel] => Get(channel);

    private readonly List<Channel> _channels = new();

    internal void Update(RoomstateArgs args)
    {
        Channel? channel = _channels.FirstOrDefault(c => c.Id == args.ChannelId);
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
        Channel? channel = _channels.FirstOrDefault(c => string.Equals(name, c.Name, StringComparison.OrdinalIgnoreCase));
        if (channel is null)
        {
            return;
        }

        _channels.Remove(channel);
    }

    internal void Remove(long id)
    {
        Channel? channel = _channels.FirstOrDefault(c => id == c.Id);
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
        return _channels.FirstOrDefault(c => c.Id == id);
    }

    private Channel? Get(string name)
    {
        return _channels.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
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
