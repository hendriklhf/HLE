using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Twitch.Api.Models;

namespace HLE.Twitch.Api;

public sealed partial class TwitchApi : IEquatable<TwitchApi>
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

    private static int GetHttpResponseContentLength(HttpResponseMessage httpResponse)
    {
        int contentLength = (int)(httpResponse.Content.Headers.ContentLength ?? 0);
        if (contentLength < 0)
        {
            throw new InvalidOperationException("The HTTP response content has a length of less than 0 or is null.");
        }

        return contentLength;
    }

    private static async ValueTask<ReadOnlyMemory<byte>> GetHttpContentBytes(HttpResponseMessage httpResponse, byte[] buffer, int contentLength)
    {
        MemoryStream memoryStream = new(buffer);
        await httpResponse.Content.LoadIntoBufferAsync(contentLength);
        await httpResponse.Content.CopyToAsync(memoryStream);
        return buffer.AsMemory(0, contentLength);
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

        int contentLength = GetHttpResponseContentLength(httpResponse);
        using RentedArray<byte> httpContentBuffer = new(contentLength);
        ReadOnlyMemory<byte> httpContent = await GetHttpContentBytes(httpResponse, httpContentBuffer, contentLength);
        return JsonSerializer.Deserialize<AccessToken>(httpContent.Span);
    }

    private async ValueTask EnsureValidAccessToken()
    {
        if (_accessToken != AccessToken.Empty && _accessToken.IsValid)
        {
            return;
        }

        _accessToken = await GetAccessTokenAsync();
    }

    private async ValueTask<HttpResponse> ExecuteRequest(string url)
    {
        using HttpClient httpClient = await CreateHttpClientAsync();
        using HttpResponseMessage httpResponse = await httpClient.GetAsync(url);

        int contentLength = GetHttpResponseContentLength(httpResponse);
        RentedArray<byte> httpContentBuffer = new(contentLength);
        ReadOnlyMemory<byte> httpContent = await GetHttpContentBytes(httpResponse, httpContentBuffer, contentLength);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"The request failed with code {httpResponse.StatusCode} and delivered: {Encoding.UTF8.GetString(httpContent.Span)}");
        }

        return new(httpContentBuffer, contentLength);
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
