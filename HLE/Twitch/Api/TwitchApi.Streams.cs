using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Twitch.Api.Models;
using HLE.Twitch.Api.Models.Responses;

namespace HLE.Twitch.Api;

public sealed partial class TwitchApi
{
    public async ValueTask<Stream?> GetStreamAsync(long userId)
    {
        if (TryGetStreamFromCache(userId, out Stream? stream))
        {
            return stream;
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "streams", _apiBaseUrl.Length + "streams".Length + 50);
        urlBuilder.AppendParameter("user_id", userId);
        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetStreamsResponse getStreamsResponse = JsonSerializer.Deserialize<GetStreamsResponse>(response.Bytes.Span);
        if (getStreamsResponse.Streams.Length == 0)
        {
            return null;
        }

        stream = getStreamsResponse.Streams[0];
        Cache?.AddStream(stream);
        return stream;
    }

    public async ValueTask<Stream?> GetStreamAsync(string username)
    {
        return await GetStreamAsync(username.AsMemory());
    }

    public async ValueTask<Stream?> GetStreamAsync(ReadOnlyMemory<char> username)
    {
        if (TryGetStreamFromCache(username.Span, out Stream? stream))
        {
            return stream;
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "streams", _apiBaseUrl.Length + "streams".Length + 50);
        urlBuilder.AppendParameter("user_login", username.Span);
        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetStreamsResponse getStreamsResponse = JsonSerializer.Deserialize<GetStreamsResponse>(response.Bytes.Span);
        if (getStreamsResponse.Streams.Length == 0)
        {
            return null;
        }

        stream = getStreamsResponse.Streams[0];
        Cache?.AddStream(stream);
        return stream;
    }

    public async ValueTask<Stream[]> GetStreamsAsync(IEnumerable<string> usernames)
    {
        return await GetStreamsAsync(usernames.ToArray(), ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<Stream[]> GetStreamsAsync(IEnumerable<long> userIds)
    {
        return await GetStreamsAsync(ReadOnlyMemory<string>.Empty, userIds.ToArray());
    }

    public async ValueTask<Stream[]> GetStreamsAsync(IEnumerable<string> usernames, IEnumerable<long> userIds)
    {
        return await GetStreamsAsync(usernames.ToArray(), userIds.ToArray());
    }

    public async ValueTask<Stream[]> GetStreamsAsync(List<string> usernames)
    {
        return await GetStreamsAsync(CollectionsMarshal.AsSpan(usernames).AsMemoryDangerous(), ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<Stream[]> GetStreamsAsync(List<long> userIds)
    {
        return await GetStreamsAsync(ReadOnlyMemory<string>.Empty, CollectionsMarshal.AsSpan(userIds).AsMemoryDangerous());
    }

    public async ValueTask<Stream[]> GetStreamsAsync(List<string> usernames, List<long> userIds)
    {
        return await GetStreamsAsync(CollectionsMarshal.AsSpan(usernames).AsMemoryDangerous(), CollectionsMarshal.AsSpan(userIds).AsMemoryDangerous());
    }

    public async ValueTask<Stream[]> GetStreamsAsync(params string[] usernames)
    {
        return await GetStreamsAsync(usernames, ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<Stream[]> GetStreamsAsync(params long[] userIds)
    {
        return await GetStreamsAsync(ReadOnlyMemory<string>.Empty, userIds);
    }

    public async ValueTask<Stream[]> GetStreamsAsync(string[] usernames, long[] userIds)
    {
        return await GetStreamsAsync(usernames.AsMemory(), userIds);
    }

    public async ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames)
    {
        return await GetStreamsAsync(usernames, ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<long> userIds)
    {
        return await GetStreamsAsync(ReadOnlyMemory<string>.Empty, userIds);
    }

    public async ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds)
    {
        using RentedArray<Stream> streamBuffer = new(usernames.Length + userIds.Length);
        int streamCount = await GetStreamsAsync(usernames, userIds, streamBuffer);
        return streamCount == 0 ? Array.Empty<Stream>() : streamBuffer[..streamCount].ToArray();
    }

    public async ValueTask<int> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds, Memory<Stream> resultBuffer)
    {
        int parameterCount = usernames.Length + userIds.Length;
        switch (parameterCount)
        {
            case 0:
                return 0;
            case 100:
                throw new ArgumentException("The endpoint allows only up to 100 parameters. You can't pass more than 100 usernames or user ids in total.");
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "streams", usernames.Length * 35 + userIds.Length * 25 + 50);
        int cachedStreamCount = 0;
        for (int i = 0; i < usernames.Length; i++)
        {
            string username = usernames.Span[i];
            if (TryGetStreamFromCache(username, out Stream? stream))
            {
                resultBuffer.Span[cachedStreamCount++] = stream;
                continue;
            }

            urlBuilder.AppendParameter("user_login", username);
        }

        for (int i = 0; i < userIds.Length; i++)
        {
            long userId = userIds.Span[i];
            if (TryGetStreamFromCache(userId, out Stream? stream))
            {
                resultBuffer.Span[cachedStreamCount++] = stream;
                continue;
            }

            urlBuilder.AppendParameter("user_id", userId);
        }

        if (urlBuilder.ParameterCount == 0)
        {
            return cachedStreamCount;
        }

        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetStreamsResponse getStreamsResponse = JsonSerializer.Deserialize<GetStreamsResponse>(response.Bytes.Span);
        int deserializedStreamCount = getStreamsResponse.Streams.Length;
        if (deserializedStreamCount > 0)
        {
            getStreamsResponse.Streams.CopyTo(resultBuffer.Span[cachedStreamCount..]);
        }

        Cache?.AddStreams(resultBuffer.Span[cachedStreamCount..(cachedStreamCount + deserializedStreamCount)]);
        return deserializedStreamCount + cachedStreamCount;
    }

    private bool TryGetStreamFromCache(long userId, [MaybeNullWhen(false)] out Stream stream)
    {
        if (Cache is not null)
        {
            return Cache.TryGetStream(userId, out stream);
        }

        stream = null;
        return false;
    }

    private bool TryGetStreamFromCache(ReadOnlySpan<char> username, [MaybeNullWhen(false)] out Stream stream)
    {
        if (Cache is not null)
        {
            return Cache.TryGetStream(username, out stream);
        }

        stream = null;
        return false;
    }
}
