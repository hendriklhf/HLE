using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Http;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Api.Bttv.Models;
using HLE.Twitch.Api.Bttv.Models.Responses;

namespace HLE.Twitch.Api.Bttv;

public sealed class BttvApi : IEquatable<BttvApi>
{
    public BttvApiCache? Cache { get; set; }

    private const string _apiBaseUrl = "https://api.betterttv.net/3";

    public BttvApi(CacheOptions? cacheOptions = null)
    {
        if (cacheOptions is not null)
        {
            Cache = new(cacheOptions);
        }
    }

    public async ValueTask<Emote[]?> GetChannelEmotesAsync(long channelId)
    {
        if (TryGetChannelEmotesFromCache(channelId, out Emote[]? emotes))
        {
            return emotes;
        }

        using PoolBufferStringBuilder urlBuilder = new(100);
        urlBuilder.Append(_apiBaseUrl, "/cached/users/twitch/");
        urlBuilder.Append(channelId);

        using HttpClient httpClient = new();
        using HttpResponseMessage httpResponse = await httpClient.GetAsync(urlBuilder.ToString());
        int contentLength = httpResponse.GetContentLength();
        if (contentLength == 0)
        {
            throw ThrowHelper.EmptyHttpResponseContentBody;
        }

        using HttpContentBytes httpContentBytes = await httpResponse.GetContentBytes(contentLength);
        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw ThrowHelper.HttpRequestDidntSucceed((int)httpResponse.StatusCode, httpContentBytes.Span);
        }

        GetUserResponse userResponse = JsonSerializer.Deserialize<GetUserResponse>(httpContentBytes.Span);
        if (userResponse.ChannelEmotes.Length == 0 && userResponse.SharedEmotes.Length == 0)
        {
            return Array.Empty<Emote>();
        }

        emotes = new Emote[userResponse.ChannelEmotes.Length + userResponse.SharedEmotes.Length];
        userResponse.ChannelEmotes.CopyTo(emotes.AsSpan());
        userResponse.SharedEmotes.CopyTo(emotes.AsSpan(userResponse.ChannelEmotes.Length));
        Cache?.AddChannelEmotes(channelId, emotes);
        return emotes;
    }

    public async ValueTask<Emote[]> GetGlobalEmotes()
    {
        if (TryGetGlobalEmotesFromCache(out Emote[]? emotes))
        {
            return emotes;
        }

        using PoolBufferStringBuilder urlBuilder = new(100);
        urlBuilder.Append(_apiBaseUrl, "/cached/emotes/global");

        using HttpClient httpClient = new();
        using HttpResponseMessage httpResponse = await httpClient.GetAsync(StringPool.Shared.GetOrAdd(urlBuilder.WrittenSpan));
        int contentLength = httpResponse.GetContentLength();
        if (contentLength == 0)
        {
            throw ThrowHelper.EmptyHttpResponseContentBody;
        }

        using HttpContentBytes httpContentBytes = await httpResponse.GetContentBytes(contentLength);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw ThrowHelper.HttpRequestDidntSucceed((int)httpResponse.StatusCode, httpContentBytes.Span);
        }

        emotes = JsonSerializer.Deserialize<Emote[]>(httpContentBytes.Span) ?? throw new InvalidOperationException("The deserialization of the global emotes response failed and returned null.");
        Cache?.AddGlobalEmotes(emotes);
        return emotes;
    }

    private bool TryGetChannelEmotesFromCache(long channelId, [MaybeNullWhen(false)] out Emote[] emotes)
    {
        emotes = null;
        return Cache?.TryGetChannelEmotes(channelId, out emotes) == true;
    }

    private bool TryGetGlobalEmotesFromCache([MaybeNullWhen(false)] out Emote[] emotes)
    {
        emotes = null;
        return Cache?.TryGetGlobalEmotes(out emotes) == true;
    }

    public bool Equals(BttvApi? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is BttvApi other && Equals(other);
    }

    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }

    public static bool operator ==(BttvApi? left, BttvApi? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BttvApi? left, BttvApi? right)
    {
        return !(left == right);
    }
}