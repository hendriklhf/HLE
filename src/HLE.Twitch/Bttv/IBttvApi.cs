using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HLE.Twitch.Bttv.Models;

namespace HLE.Twitch.Bttv;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface IBttvApi
{
    ValueTask<ImmutableArray<Emote>> GetChannelEmotesAsync(long channelId);

    ValueTask<ImmutableArray<Emote>> GetGlobalEmotesAsync();
}
