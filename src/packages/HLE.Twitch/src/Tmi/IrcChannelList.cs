using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace HLE.Twitch.Tmi;

internal sealed class IrcChannelList : IEquatable<IrcChannelList>
{
    private readonly List<IrcChannel> _channels = new(8);
    private ReadOnlyMemory<byte>[]? _utf8NamesCache;
#if NET9_0_OR_GREATER
    private readonly Lock _utf8NamesLock = new();
#else
    private readonly object _utf8NamesLock = new();
#endif

    [Pure]
    public ImmutableArray<ReadOnlyMemory<byte>> GetUtf8Names()
    {
        ReadOnlyMemory<byte>[]? utf8Names = _utf8NamesCache;
        if (utf8Names is not null)
        {
            return ImmutableCollectionsMarshal.AsImmutableArray(utf8Names);
        }

        lock (_utf8NamesLock)
        {
            utf8Names = _utf8NamesCache;
            if (utf8Names is not null)
            {
                return ImmutableCollectionsMarshal.AsImmutableArray(utf8Names);
            }

            utf8Names = new ReadOnlyMemory<byte>[_channels.Count];
            ReadOnlySpan<IrcChannel> channels = CollectionsMarshal.AsSpan(_channels);
            for (int i = 0; i < channels.Length; i++)
            {
                utf8Names[i] = channels[i].NameUtf8.AsMemory();
            }

            _utf8NamesCache = utf8Names;
            return ImmutableCollectionsMarshal.AsImmutableArray(utf8Names);
        }
    }

    public IrcChannel Add(ReadOnlySpan<char> name)
    {
        string formattedName = ChannelFormatter.Format(name, true);

        List<IrcChannel> channels = _channels;
        lock (channels)
        {
            if (TryGet(CollectionsMarshal.AsSpan(channels), formattedName, out IrcChannel? channel))
            {
                return channel;
            }

            channel = new(formattedName);
            channels.Add(channel);
            InvalidateUtf8NamesCache();
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
            if (!TryGet(CollectionsMarshal.AsSpan(channels), formattedName, out IrcChannel? channel))
            {
                return null;
            }

            channels.Remove(channel);
            InvalidateUtf8NamesCache();

            return channel;
        }
    }

    public void Clear()
    {
        lock (_channels)
        {
            _channels.Clear();
        }

        InvalidateUtf8NamesCache();
    }

    private bool TryGet(ReadOnlySpan<IrcChannel> channels, string name, [MaybeNullWhen(false)] out IrcChannel channel)
    {
        Debug.Assert(Monitor.IsEntered(_channels));

        for (int i = 0; i < channels.Length; i++)
        {
            IrcChannel ircChannel = channels[i];
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

    private void InvalidateUtf8NamesCache()
    {
        lock (_utf8NamesLock)
        {
            _utf8NamesCache = null;
        }
    }

    public bool Equals([NotNullWhen(true)] IrcChannelList? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(IrcChannelList? left, IrcChannelList? right) => Equals(left, right);

    public static bool operator !=(IrcChannelList? left, IrcChannelList? right) => !(left == right);
}
