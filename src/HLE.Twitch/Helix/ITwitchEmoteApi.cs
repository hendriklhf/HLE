using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HLE.Twitch.Helix.Models;

namespace HLE.Twitch.Helix;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface ITwitchEmoteApi
{
    ValueTask<ImmutableArray<Emote>> GetGlobalEmotesAsync();

    ValueTask<ImmutableArray<ChannelEmote>> GetChannelEmotesAsync(long channelId);
}
