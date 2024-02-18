using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HLE.Twitch.Helix.Models;

namespace HLE.Twitch.Helix;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface ITwitchStreamApi
{
    ValueTask<Stream?> GetStreamAsync(long userId);

    ValueTask<Stream?> GetStreamAsync(string username);

    ValueTask<Stream?> GetStreamAsync(ReadOnlyMemory<char> username);

    ValueTask<Stream[]> GetStreamsAsync(IEnumerable<string> usernames);

    ValueTask<Stream[]> GetStreamsAsync(IEnumerable<long> channelIds);

    ValueTask<Stream[]> GetStreamsAsync(List<string> usernames);

    ValueTask<Stream[]> GetStreamsAsync(List<long> channelIds);

    ValueTask<Stream[]> GetStreamsAsync(params string[] usernames);

    ValueTask<Stream[]> GetStreamsAsync(string[] usernames, long[] channelIds);

    ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames);

    ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> channelIds);

    ValueTask<int> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> channelIds, Stream[] destination);
}
