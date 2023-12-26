using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

internal sealed class IrcChannelList : IEquatable<IrcChannelList>
{
    private readonly List<IrcChannel> _channels = new(8);
    private ReadOnlyMemory<byte>[]? _utf8NamesCache;

    [Pure]
    public ImmutableArray<ReadOnlyMemory<byte>> GetUtf8Names()
    {
        ReadOnlyMemory<byte>[]? utf8Names = _utf8NamesCache;
        if (utf8Names is not null)
        {
            return ImmutableCollectionsMarshal.AsImmutableArray(utf8Names);
        }

        utf8Names = _channels.Select(static c => c.NameUtf8.AsMemory()).ToArray();
        _utf8NamesCache = utf8Names;
        return ImmutableCollectionsMarshal.AsImmutableArray(utf8Names);
    }

    public IrcChannel Add(ReadOnlySpan<char> name)
    {
        string formattedName = ChannelFormatter.Format(name, true);

        List<IrcChannel> channels = _channels;
        lock (channels)
        {
            if (TryGet(channels, formattedName, out IrcChannel? channel))
            {
                return channel;
            }

            channel = new(formattedName);
            channels.Add(channel);
            _utf8NamesCache = null;
            return channel;
        }
    }

    public IrcChannel? Remove(ReadOnlySpan<char> name)
    {
        List<IrcChannel> channels = _channels;
        if (channels.Count == 0)
        {
            return null;
        }

        string formattedName = ChannelFormatter.Format(name, true);

        lock (channels)
        {
            if (!TryGet(channels, formattedName, out IrcChannel? channel))
            {
                return null;
            }

            channels.Remove(channel);
            _utf8NamesCache = null;
            return channel;
        }
    }

    public void Clear()
    {
        _channels.Clear();
        _utf8NamesCache = null;
    }

    private static bool TryGet(List<IrcChannel> channels, string name, [MaybeNullWhen(false)] out IrcChannel channel)
    {
        Debug.Assert(Monitor.IsEntered(channels));

        Span<IrcChannel> channelsSpan = CollectionsMarshal.AsSpan(channels);
        for (int i = 0; i < channelsSpan.Length; i++)
        {
            IrcChannel ircChannel = channelsSpan[i];
            if (ircChannel.Name != name)
            {
                continue;
            }

            channel = ircChannel;
            return true;
        }

        channel = null;
        return false;
    }

    public bool Equals([NotNullWhen(true)] IrcChannelList? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(IrcChannelList? left, IrcChannelList? right) => Equals(left, right);

    public static bool operator !=(IrcChannelList? left, IrcChannelList? right) => !(left == right);
}
