using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Twitch.Api.Models;
using HLE.Twitch.Api.Models.Responses;

namespace HLE.Twitch.Api;

public sealed partial class TwitchApi
{
    public async ValueTask<Emote[]> GetGlobalEmotesAsync()
    {
        if (TryGetGlobalEmotes(out Emote[]? emotes))
        {
            return emotes;
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "chat/emotes/global");
        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetResponse<Emote> getResponse = JsonSerializer.Deserialize<GetResponse<Emote>>(response.Bytes.Span);
        if (getResponse.Items.Length == 0)
        {
            throw new InvalidOperationException("An unknown error occurred. The response contained zero emotes.");
        }

        emotes = getResponse.Items;
        Cache?.AddGlobalEmotes(emotes);
        return emotes;
    }

    public async ValueTask<ChannelEmote[]> GetChannelEmotesAsync(long channelId)
    {
        if (TryGetChannelEmotes(channelId, out ChannelEmote[]? emotes))
        {
            return emotes;
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "chat/emotes");
        urlBuilder.AppendParameter("broadcaster_id", channelId);
        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetResponse<ChannelEmote> getResponse = JsonSerializer.Deserialize<GetResponse<ChannelEmote>>(response.Bytes.Span);
        emotes = getResponse.Items;
        Cache?.AddChannelEmotes(channelId, emotes);
        return emotes;
    }

    private bool TryGetGlobalEmotes([MaybeNullWhen(false)] out Emote[] globalEmotes)
    {
        if (Cache is not null)
        {
            return Cache.TryGetGlobalEmotes(out globalEmotes);
        }

        globalEmotes = null;
        return false;
    }

    private bool TryGetChannelEmotes(long channelId, [MaybeNullWhen(false)] out ChannelEmote[] channelEmotes)
    {
        if (Cache is not null)
        {
            return Cache.TryGetChannelEmotes(channelId, out channelEmotes);
        }

        channelEmotes = null;
        return false;
    }
}
