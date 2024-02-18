using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Twitch.Helix.Models;

namespace HLE.Twitch.Helix;

public sealed partial class TwitchApi : ITwitchApi, IEquatable<TwitchApi>, IDisposable
{
    public TwitchApiCache? Cache { get; set; }

    private readonly string _clientId;
    private AccessToken _accessToken = AccessToken.Empty;
    private string? _bearer;
    private readonly FormUrlEncodedContent _accessTokenRequestContent;

    private const string ApiBaseUrl = "https://api.twitch.tv/helix";

    public TwitchApi(string clientId, string clientSecret, CacheOptions? cacheOptions = null)
    {
        _clientId = clientId;
        if (cacheOptions is not null)
        {
            Cache = new(cacheOptions);
        }

        _accessTokenRequestContent = new
        ([
            new("client_id", _clientId),
            new("client_secret", clientSecret),
            new("grant_type", "client_credentials")
        ]);
    }

    public void Dispose() => _accessTokenRequestContent.Dispose();

    private async ValueTask<HttpClient> CreateHttpClientAsync()
    {
        await EnsureValidAccessTokenAsync();
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("Client-Id", _clientId);
        httpClient.DefaultRequestHeaders.Add("Authorization", _bearer);
        return httpClient;
    }

    public async ValueTask<AccessToken> GetAccessTokenAsync()
    {
        using HttpClient httpClient = new();
        using HttpResponseMessage httpResponse = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", _accessTokenRequestContent);
        using HttpContentBytes httpContentBytes = await HttpContentBytes.CreateAsync(httpResponse);
        if (httpContentBytes.Length == 0)
        {
            throw new HttpResponseEmptyException();
        }

        return JsonSerializer.Deserialize(httpContentBytes.AsSpan(), HelixJsonSerializerContext.Default.AccessToken);
    }

    private async ValueTask EnsureValidAccessTokenAsync()
    {
        AccessToken accessToken = _accessToken;
        if (accessToken != AccessToken.Empty && accessToken.IsValid)
        {
            Debug.Assert(_bearer is not null);
            return;
        }

        accessToken = await GetAccessTokenAsync();
        _bearer = $"Bearer {accessToken}";
        _accessToken = accessToken;
    }

    private async ValueTask<HttpContentBytes> ExecuteRequestAsync(string url)
    {
        using HttpClient httpClient = await CreateHttpClientAsync();
        using HttpResponseMessage httpResponse = await httpClient.GetAsync(url);
        HttpContentBytes httpContentBytes = await HttpContentBytes.CreateAsync(httpResponse);
        if (httpContentBytes.Length == 0)
        {
            throw new HttpResponseEmptyException();
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new HttpRequestFailedException(httpResponse.StatusCode, httpContentBytes.AsSpan());
        }

        return httpContentBytes;
    }

    public bool Equals(TwitchApi? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => obj is TwitchApi other && Equals(other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(TwitchApi? left, TwitchApi? right) => Equals(left, right);

    public static bool operator !=(TwitchApi? left, TwitchApi? right) => !(left == right);
}
