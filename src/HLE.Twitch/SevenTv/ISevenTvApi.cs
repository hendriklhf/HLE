using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HLE.Twitch.SevenTv.Models;

namespace HLE.Twitch.SevenTv;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface ISevenTvApi
{
    ValueTask<ImmutableArray<Emote>> GetGlobalEmotesAsync();

    ValueTask<ImmutableArray<Emote>> GetChannelEmotesAsync(long channelId);
}
