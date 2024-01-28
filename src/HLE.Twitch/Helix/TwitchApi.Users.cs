using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Twitch.Helix.Models;
using HLE.Twitch.Helix.Models.Responses;

namespace HLE.Twitch.Helix;

public sealed partial class TwitchApi
{
    public async ValueTask<User?> GetUserAsync(long userId)
    {
        if (TryGetUserFromCache(userId, out User? user))
        {
            return user;
        }

        using UrlBuilder urlBuilder = new(ApiBaseUrl, "users", ApiBaseUrl.Length + "users".Length + 50);
        urlBuilder.AppendParameter("id", userId);
        using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
        GetResponse<User> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseUser);
        if (getResponse.Items.Length == 0)
        {
            return null;
        }

        user = getResponse.Items[0];
        Cache?.AddUser(user);
        return user;
    }

    // ReSharper disable once InconsistentNaming
    public ValueTask<User?> GetUserAsync(string username) => GetUserAsync(username.AsMemory());

    public async ValueTask<User?> GetUserAsync(ReadOnlyMemory<char> username)
    {
        if (TryGetUserFromCache(username.Span, out User? user))
        {
            return user;
        }

        using UrlBuilder urlBuilder = new(ApiBaseUrl, "users", ApiBaseUrl.Length + "users".Length + 50);
        urlBuilder.AppendParameter("login", username.Span);
        using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
        GetResponse<User> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseUser);
        if (getResponse.Items.Length == 0)
        {
            return null;
        }

        user = getResponse.Items[0];
        Cache?.AddUser(user);
        return user;
    }

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<string> usernames)
        => GetUsersAsync(usernames.TryGetReadOnlyMemory<string>(out ReadOnlyMemory<string> usernamesMemory) ? usernamesMemory : usernames.ToArray(), ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<long> userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> userIdsMemory) ? userIdsMemory : userIds.ToArray());

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<string> usernames, IEnumerable<long> userIds)
    {
        bool usernamesIsMemory = usernames.TryGetReadOnlyMemory<string>(out ReadOnlyMemory<string> usernamesMemory);
        bool userIdsIsMemory = userIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> userIdsMemory);

        return usernamesIsMemory switch
        {
            true when userIdsIsMemory => GetUsersAsync(usernamesMemory, userIdsMemory),
            true when !userIdsIsMemory => GetUsersAsync(usernamesMemory, userIds.ToArray()),
            false when userIdsIsMemory => GetUsersAsync(usernamesMemory.ToArray(), userIdsMemory),
            _ => GetUsersAsync(usernames.ToArray(), userIds.ToArray())
        };
    }

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(List<string> usernames)
        => GetUsersAsync(SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(usernames)), ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(List<long> userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(userIds)));

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(List<string> usernames, List<long> userIds)
        => GetUsersAsync(SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(usernames)), SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(userIds)));

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(params string[] usernames)
        => GetUsersAsync(usernames, ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(params long[] userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds);

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(string[] usernames, long[] userIds)
        => GetUsersAsync(usernames.AsMemory(), userIds);

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<string> usernames)
        => GetUsersAsync(usernames, ReadOnlyMemory<long>.Empty);

    // ReSharper disable once InconsistentNaming
    public ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<long> userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds);

    public async ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds)
    {
        using RentedArray<User> buffer = ArrayPool<User>.Shared.RentAsRentedArray(usernames.Length + userIds.Length);
        int userCount = await GetUsersAsync(usernames, userIds, RentedArrayMarshal.GetArray(buffer));
        return userCount == 0 ? [] : ImmutableCollectionsMarshal.AsImmutableArray(buffer.ToArray(..userCount));
    }

    public async ValueTask<int> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds, User[] destination)
    {
        int parameterCount = usernames.Length + userIds.Length;
        switch (parameterCount)
        {
            case 0:
                return 0;
            case > 100:
                throw new ArgumentException("The endpoint allows only up to 100 parameters. You can't pass more than 100 usernames or user ids in total.");
        }

        using UrlBuilder urlBuilder = new(ApiBaseUrl, "users", usernames.Length * 35 + userIds.Length * 25 + 50);
        int cachedUserCount = 0;
        for (int i = 0; i < usernames.Length; i++)
        {
            string username = usernames.Span[i];
            if (TryGetUserFromCache(username, out User? user))
            {
                destination[cachedUserCount++] = user;
                continue;
            }

            urlBuilder.AppendParameter("login", username);
        }

        for (int i = 0; i < userIds.Length; i++)
        {
            long userId = userIds.Span[i];
            if (TryGetUserFromCache(userId, out User? user))
            {
                destination[cachedUserCount++] = user;
                continue;
            }

            urlBuilder.AppendParameter("id", userId);
        }

        if (urlBuilder.ParameterCount == 0)
        {
            return cachedUserCount;
        }

        using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
        GetResponse<User> getResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.GetResponseUser);
        int deserializedUserCount = getResponse.Items.Length;
        if (deserializedUserCount != 0)
        {
            getResponse.Items.CopyTo(destination[cachedUserCount..]);
        }

        Cache?.AddUsers(destination.AsSpan(cachedUserCount..(cachedUserCount + deserializedUserCount)));
        return deserializedUserCount + cachedUserCount;
    }

    private bool TryGetUserFromCache(long userId, [MaybeNullWhen(false)] out User user)
    {
        user = null;
        return Cache?.TryGetUser(userId, out user) == true;
    }

    private bool TryGetUserFromCache(ReadOnlySpan<char> username, [MaybeNullWhen(false)] out User user)
    {
        user = null;
        return Cache?.TryGetUser(username, out user) == true;
    }
}
