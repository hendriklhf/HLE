using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HLE.Twitch.Ffz.Models;

namespace HLE.Twitch.Ffz;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface IFfzApi
{
    ValueTask<ImmutableArray<Emote>> GetChannelEmotesAsync(long channelId);

    ValueTask<ImmutableArray<Emote>> GetChannelEmotesAsync(string channelName);

    ValueTask<ImmutableArray<Emote>> GetChannelEmotesAsync(ReadOnlyMemory<char> channelName);

    ValueTask<ImmutableArray<Emote>> GetGlobalEmotesAsync();
}
