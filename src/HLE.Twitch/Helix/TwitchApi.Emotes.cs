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
            HelixResponse<Emote> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseEmote);
            if (helixResponse.Items.Length == 0)
            {
                throw new InvalidOperationException("An unknown error occurred. The response contained zero emotes.");
            }

            ImmutableArray<Emote> emotes = helixResponse.Items;
            Cache?.AddGlobalEmotes(emotes);
            return emotes;
        }
    }

    public async ValueTask<ImmutableArray<ChannelEmote>> GetChannelEmotesAsync(ReadOnlyMemory<char> username)
    {
        User user = await GetUserAsync(username) ?? throw new InvalidOperationException($"The user {username} does not exist.");
        return await GetChannelEmotesAsync(user.Id);
    }

    public ValueTask<ImmutableArray<ChannelEmote>> GetChannelEmotesAsync(long channelId)
    {
        return TryGetChannelEmotesFromCache(channelId, out ImmutableArray<ChannelEmote> emotes)
            ? ValueTask.FromResult(emotes)
            : GetChannelEmotesCoreAsync(channelId);

        async ValueTask<ImmutableArray<ChannelEmote>> GetChannelEmotesCoreAsync(long channelId)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, "chat/emotes");
            urlBuilder.AppendParameter("broadcaster_id", channelId);
            using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
            HelixResponse<ChannelEmote> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseChannelEmote);
            ImmutableArray<ChannelEmote> emotes = helixResponse.Items;
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
