using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Twitch.Helix.Models;
using HLE.Twitch.Helix.Models.Responses;

namespace HLE.Twitch.Helix;

public sealed partial class TwitchApi
{
    public ValueTask<ImmutableArray<Emote>> GetGlobalEmotesAsync()
    {
        return TryGetGlobalEmotesFromCache(out ImmutableArray<Emote> emotes)
            ? ValueTask.FromResult(emotes)
            : GetGlobalEmotesCoreAsync();

        async ValueTask<ImmutableArray<Emote>> GetGlobalEmotesCoreAsync()
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, "chat/emotes/global");
            using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
            GetResponse<Emote> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseEmote);
            if (getResponse.Items.Length == 0)
            {
                throw new InvalidOperationException("An unknown error occurred. The response contained zero emotes.");
            }

            ImmutableArray<Emote> emotes = getResponse.Items;
            Cache?.AddGlobalEmotes(emotes);
            return emotes;
        }
    }

    public ValueTask<ImmutableArray<ChannelEmote>> GetChannelEmotesAsync(long channelId)
    {
        return TryGetChannelEmotesFromCache(channelId, out ImmutableArray<ChannelEmote> emotes)
            ? ValueTask.FromResult(emotes)
            : GetChannelEmotesCoreAsync(channelId);

        // ReSharper disable once InconsistentNaming
        async ValueTask<ImmutableArray<ChannelEmote>> GetChannelEmotesCoreAsync(long channelId)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, "chat/emotes");
            urlBuilder.AppendParameter("broadcaster_id", channelId);
            using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
            GetResponse<ChannelEmote> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseChannelEmote);
            ImmutableArray<ChannelEmote> emotes = getResponse.Items;
            Cache?.AddChannelEmotes(channelId, emotes);
            return emotes;
        }
    }

    private bool TryGetGlobalEmotesFromCache(out ImmutableArray<Emote> emotes)
    {
        emotes = [];
        return Cache?.TryGetGlobalEmotes(out emotes) == true;
    }

    private bool TryGetChannelEmotesFromCache(long channelId, out ImmutableArray<ChannelEmote> emotes)
    {
        emotes = [];
        return Cache?.TryGetChannelEmotes(channelId, out emotes) == true;
    }
}
