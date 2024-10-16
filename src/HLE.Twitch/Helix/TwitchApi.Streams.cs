using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    private const string StreamsEndpoint = "streams";

    public ValueTask<Stream?> GetStreamAsync(long userId)
    {
        return TryGetStreamFromCache(userId, out Stream? stream)
            ? ValueTask.FromResult<Stream?>(stream)
            : GetStreamCoreAsync(userId);

        async ValueTask<Stream?> GetStreamCoreAsync(long userId)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, StreamsEndpoint, ApiBaseUrl.Length + StreamsEndpoint.Length + 50);
            urlBuilder.AppendParameter("user_id", userId);
            using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
            HelixResponse<Stream> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseStream);
            if (helixResponse.Items.Length == 0)
            {
                return null;
            }

            stream = helixResponse.Items[0];
            Cache?.AddStream(stream);
            return stream;
        }
    }

    public ValueTask<Stream?> GetStreamAsync(string username) => GetStreamAsync(username.AsMemory());

    public ValueTask<Stream?> GetStreamAsync(ReadOnlyMemory<char> username)
    {
        return TryGetStreamFromCache(username.Span, out Stream? stream)
            ? ValueTask.FromResult<Stream?>(stream)
            : GetStreamCoreAsync(username);

        async ValueTask<Stream?> GetStreamCoreAsync(ReadOnlyMemory<char> username)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, StreamsEndpoint, ApiBaseUrl.Length + StreamsEndpoint.Length + 50);
            urlBuilder.AppendParameter("user_login", username.Span);
            using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
            HelixResponse<Stream> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseStream);
            if (helixResponse.Items.Length == 0)
            {
                return null;
            }

            stream = helixResponse.Items[0];
            Cache?.AddStream(stream);
            return stream;
        }
    }

    public ValueTask<Stream[]> GetStreamsAsync(IEnumerable<string> usernames)
        => GetStreamsAsync(usernames.TryGetReadOnlyMemory(out ReadOnlyMemory<string> usernamesMemory) ? usernamesMemory : usernames.ToArray(), ReadOnlyMemory<long>.Empty);

    public ValueTask<Stream[]> GetStreamsAsync(IEnumerable<long> channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, channelIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> channelIdsMemory) ? channelIdsMemory : channelIds.ToArray());

    public ValueTask<Stream[]> GetStreamsAsync(IEnumerable<string> usernames, IEnumerable<long> channelIds)
    {
        bool usernamesIsMemory = usernames.TryGetReadOnlyMemory(out ReadOnlyMemory<string> usernamesMemory);
        bool channelIdsIsMemory = channelIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> channelIdsMemory);

        return usernamesIsMemory switch
        {
            true when channelIdsIsMemory => GetStreamsAsync(usernamesMemory, channelIdsMemory),
            true when !channelIdsIsMemory => GetStreamsAsync(usernamesMemory, channelIds.ToArray()),
            false when channelIdsIsMemory => GetStreamsAsync(usernamesMemory.ToArray(), channelIdsMemory),
            _ => GetStreamsAsync(usernames.ToArray(), channelIds.ToArray())
        };
    }

    public ValueTask<Stream[]> GetStreamsAsync(List<string> usernames)
        => GetStreamsAsync(ListMarshal.AsMemory(usernames), ReadOnlyMemory<long>.Empty);

    public ValueTask<Stream[]> GetStreamsAsync(List<long> channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, ListMarshal.AsMemory(channelIds));

    public ValueTask<Stream[]> GetStreamsAsync(List<string> usernames, List<long> channelIds)
        => GetStreamsAsync(ListMarshal.AsMemory(usernames), ListMarshal.AsMemory(channelIds));

    public ValueTask<Stream[]> GetStreamsAsync(params string[] usernames)
        => GetStreamsAsync(usernames, ReadOnlyMemory<long>.Empty);

    public ValueTask<Stream[]> GetStreamsAsync(params long[] channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, channelIds);

    public ValueTask<Stream[]> GetStreamsAsync(string[] usernames, long[] channelIds)
        => GetStreamsAsync(usernames.AsMemory(), channelIds);

    public ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames)
        => GetStreamsAsync(usernames, ReadOnlyMemory<long>.Empty);

    public ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<long> channelIds)
        => GetStreamsAsync(ReadOnlyMemory<string>.Empty, channelIds);

    public async ValueTask<Stream[]> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> channelIds)
    {
        using RentedArray<Stream> buffer = ArrayPool<Stream>.Shared.RentAsRentedArray(usernames.Length + channelIds.Length);
        int streamCount = await GetStreamsAsync(usernames, channelIds, RentedArrayMarshal.GetArray(buffer));
        return streamCount == 0 ? [] : buffer.ToArray(..streamCount);
    }

    public ValueTask<int> GetStreamsAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> channelIds, Stream[] destination)
    {
        int parameterCount = usernames.Length + channelIds.Length;
        return parameterCount switch
        {
            0 => ValueTask.FromResult(0),
            > 100 => throw new ArgumentException("The endpoint allows only up to 100 parameters. You can't pass more than 100 usernames or user ids in total."),
            _ => GetStreamsCoreAsync(usernames, channelIds, destination)
        };

        async ValueTask<int> GetStreamsCoreAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> channelIds, Stream[] destination)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, StreamsEndpoint, usernames.Length * 35 + channelIds.Length * 25 + 50);
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
            HelixResponse<Stream> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseStream);
            int deserializedStreamCount = helixResponse.Items.Length;
            if (deserializedStreamCount != 0)
            {
                helixResponse.Items.CopyTo(destination[cachedStreamCount..]);
            }

            Cache?.AddStreams(destination.AsSpan(cachedStreamCount..(cachedStreamCount + deserializedStreamCount)));
            return deserializedStreamCount + cachedStreamCount;
        }
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
