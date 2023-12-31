using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Twitch.Helix.Models.Responses;
using Stream = HLE.Twitch.Helix.Models.Stream;

namespace HLE.Twitch.Helix;

public sealed partial class TwitchApi
{
    public async ValueTask<Stream?> GetStreamAsync(long userId)
    {
        if (TryGetStreamFromCache(userId, out Stream? stream))
        {
            return stream;
        }

        using UrlBuilder urlBuilder = new(ApiBaseUrl, "streams", ApiBaseUrl.Length + "streams".Length + 50);
        urlBuilder.AppendParameter("user_id", userId);
        using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
        GetResponse<Stream> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseStream);
        if (getResponse.Items.Length == 0)
        {
            return null;
        }

        stream = getResponse.Items[0];
        Cache?.AddStream(stream);
        return stream;
    }

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream?> GetStreamAsync(string username) => GetStreamAsync(username.AsMemory());

    public async ValueTask<Stream?> GetStreamAsync(ReadOnlyMemory<char> username)
    {
        if (TryGetStreamFromCache(username.Span, out Stream? stream))
        {
            return stream;
        }

        using UrlBuilder urlBuilder = new(ApiBaseUrl, "streams", ApiBaseUrl.Length + "streams".Length + 50);
        urlBuilder.AppendParameter("user_login", username.Span);
        using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
        GetResponse<Stream> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseStream);
        if (getResponse.Items.Length == 0)
        {
            return null;
        }

        stream = getResponse.Items[0];
        Cache?.AddStream(stream);
        return stream;
    }

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(IEnumerable<string> usernames)
        => GetStreamsAsync(usernames.TryGetReadOnlyMemory<string>(out ReadOnlyMemory<string> usernamesMemory) ? usernamesMemory : usernames.ToArray(), ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(IEnumerable<long> channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, channelIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> channelIdsMemory) ? channelIdsMemory : channelIds.ToArray());

    public async ValueTask<Stream[]> GetStreamsAsync(IEnumerable<string> usernames, IEnumerable<long> channelIds)
    {
        // ReSharper disable PossibleMultipleEnumeration
        bool usernamesIsMemory = usernames.TryGetReadOnlyMemory<string>(out ReadOnlyMemory<string> usernamesMemory);
        bool channelIdsIsMemory = channelIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> channelIdsMemory);

        return usernamesIsMemory switch
        {
            true when channelIdsIsMemory => await GetStreamsAsync(usernamesMemory, channelIdsMemory),
            true when !channelIdsIsMemory => await GetStreamsAsync(usernamesMemory, channelIds.ToArray()),
            false when channelIdsIsMemory => await GetStreamsAsync(usernamesMemory.ToArray(), channelIdsMemory),
            _ => await GetStreamsAsync(usernames.ToArray(), channelIds.ToArray())
        };
        // ReSharper restore PossibleMultipleEnumeration
    }

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(List<string> usernames)
        => GetStreamsAsync(SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(usernames)), ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(List<long> channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(channelIds)));

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(List<string> usernames, List<long> channelIds)
        => GetStreamsAsync(SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(usernames)), SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(channelIds)));

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(params string[] usernames)
        => GetStreamsAsync(usernames, ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(params long[] channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, channelIds);

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(string[] usernames, long[] channelIds)
        => GetStreamsAsync(usernames.AsMemory(), channelIds);

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames)
        => GetStreamsAsync(usernames, ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<long> channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, channelIds);

    public async ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> channelIds)
    {
        using RentedArray<Stream> buffer = ArrayPool<Stream>.Shared.RentAsRentedArray(usernames.Length + channelIds.Length);
        int streamCount = await GetStreamsAsync(usernames, channelIds, RentedArrayMarshal.GetArray(buffer));
        return streamCount == 0 ? [] : buffer.ToArray(..streamCount);
    }

    public async ValueTask<int> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> channelIds, Stream[] destination)
    {
        int parameterCount = usernames.Length + channelIds.Length;
        switch (parameterCount)
        {
            case 0:
                return 0;
            case 100:
                throw new ArgumentException("The endpoint allows only up to 100 parameters. You can't pass more than 100 usernames or user ids in total.");
        }

        using UrlBuilder urlBuilder = new(ApiBaseUrl, "streams", usernames.Length * 35 + channelIds.Length * 25 + 50);
        int cachedStreamCount = 0;
        for (int i = 0; i < usernames.Length; i++)
        {
            string username = usernames.Span[i];
            if (TryGetStreamFromCache(username, out Stream? stream))
            {
                destination[cachedStreamCount++] = stream;
                continue;
            }

            urlBuilder.AppendParameter("user_login", username);
        }

        for (int i = 0; i < channelIds.Length; i++)
        {
            long userId = channelIds.Span[i];
            if (TryGetStreamFromCache(userId, out Stream? stream))
            {
                destination[cachedStreamCount++] = stream;
                continue;
            }

            urlBuilder.AppendParameter("user_id", userId);
        }

        if (urlBuilder.ParameterCount == 0)
        {
            return cachedStreamCount;
        }

        using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
        GetResponse<Stream> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseStream);
        int deserializedStreamCount = getResponse.Items.Length;
        if (deserializedStreamCount != 0)
        {
            getResponse.Items.CopyTo(destination[cachedStreamCount..]);
        }

        Cache?.AddStreams(destination.AsSpan(cachedStreamCount..(cachedStreamCount + deserializedStreamCount)));
        return deserializedStreamCount + cachedStreamCount;
    }

    private bool TryGetStreamFromCache(long channelId, [MaybeNullWhen(false)] out Stream stream)
    {
        stream = null;
        return Cache?.TryGetStream(channelId, out stream) == true;
    }

    private bool TryGetStreamFromCache(ReadOnlySpan<char> username, [MaybeNullWhen(false)] out Stream stream)
    {
        stream = null;
        return Cache?.TryGetStream(username, out stream) == true;
    }
}
