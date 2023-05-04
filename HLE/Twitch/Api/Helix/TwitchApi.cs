using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Http;
using HLE.Memory;
using HLE.Twitch.Api.Helix.Models;

namespace HLE.Twitch.Api.Helix;

public sealed partial class TwitchApi : IEquatable<TwitchApi>, IDisposable
{
    public TwitchApiCache? Cache { get; set; }

    private readonly string _clientId;
    private AccessToken _accessToken = AccessToken.Empty;
    private readonly FormUrlEncodedContent _accessTokenRequestContent;

    private const string _apiBaseUrl = "https://api.twitch.tv/helix";

    public TwitchApi(string clientId, string clientSecret, CacheOptions? cacheOptions = null)
    {
        _clientId = clientId;
        if (cacheOptions is not null)
        {
            Cache = new(cacheOptions);
        }

        _accessTokenRequestContent = new(new[]
        {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });
    }

    public void Dispose()
    {
        _accessTokenRequestContent.Dispose();
    }

    private async ValueTask<HttpClient> CreateHttpClientAsync()
    {
        await EnsureValidAccessToken();
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        httpClient.DefaultRequestHeaders.Add("Client-Id", _clientId);
        return httpClient;
    }

    public async ValueTask<AccessToken> GetAccessTokenAsync()
    {
        using HttpClient httpClient = new();
        using HttpResponseMessage httpResponse = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", _accessTokenRequestContent);
        int contentLength = httpResponse.GetContentLength();
        if (contentLength == 0)
        {
            throw ThrowHelper.EmptyHttpResponseContentBody;
        }

        HttpContentBytes httpContentBytes = await httpResponse.GetContentBytes(contentLength);
        return JsonSerializer.Deserialize<AccessToken>(httpContentBytes.Bytes.Span);
    }

    private async ValueTask EnsureValidAccessToken()
    {
        if (_accessToken != AccessToken.Empty && _accessToken.IsValid)
        {
            return;
        }

        _accessToken = await GetAccessTokenAsync();
    }

    private async ValueTask<HttpContentBytes> ExecuteRequest(string url)
    {
        using HttpClient httpClient = await CreateHttpClientAsync();
        using HttpResponseMessage httpResponse = await httpClient.GetAsync(url);
        int contentLength = httpResponse.GetContentLength();
        if (contentLength == 0)
        {
            throw ThrowHelper.EmptyHttpResponseContentBody;
        }

        HttpContentBytes httpContentBytes = await httpResponse.GetContentBytes(contentLength);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"The request ({url}) failed with code {httpResponse.StatusCode} and delivered: {Encoding.UTF8.GetString(httpContentBytes.Bytes.Span)}");
        }

        return httpContentBytes;
    }

    public bool Equals(TwitchApi? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is TwitchApi other && Equals(other);
    }

    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }

    public static bool operator ==(TwitchApi? left, TwitchApi? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TwitchApi? left, TwitchApi? right)
    {
        return !(left == right);
    }
}
