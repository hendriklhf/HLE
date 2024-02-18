using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HLE.Twitch.Helix.Models;

namespace HLE.Twitch.Helix;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface ITwitchApi : ITwitchEmoteApi, ITwitchStreamApi, ITwitchUserApi
{
    ValueTask<AccessToken> GetAccessTokenAsync();
}
