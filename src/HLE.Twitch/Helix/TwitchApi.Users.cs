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
    private const string UsersEndpoint = "users";

    public ValueTask<User?> GetUserAsync(long userId)
    {
        return TryGetUserFromCache(userId, out User? user)
            ? ValueTask.FromResult<User?>(user)
            : GetUserCoreAsync(userId);

        async ValueTask<User?> GetUserCoreAsync(long userId)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, UsersEndpoint, ApiBaseUrl.Length + UsersEndpoint.Length + 50);
            urlBuilder.AppendParameter("id", userId);
            using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
            HelixResponse<User> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseUser);
            if (helixResponse.Items.Length == 0)
            {
                return null;
            }

            User user = helixResponse.Items[0];
            Cache?.AddUser(user);
            return user;
        }
    }

    public ValueTask<User?> GetUserAsync(string username) => GetUserAsync(username.AsMemory());

    public ValueTask<User?> GetUserAsync(ReadOnlyMemory<char> username)
    {
        return TryGetUserFromCache(username.Span, out User? user)
            ? ValueTask.FromResult<User?>(user)
            : GetUsersCoreAsync(username);

        async ValueTask<User?> GetUsersCoreAsync(ReadOnlyMemory<char> username)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, UsersEndpoint, ApiBaseUrl.Length + UsersEndpoint.Length + 50);
            urlBuilder.AppendParameter("login", username.Span);
            using HttpContentBytes response = await ExecuteRequestAsync(urlBuilder.ToString());
            HelixResponse<User> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseUser);
            if (helixResponse.Items.Length == 0)
            {
                return null;
            }

            User user = helixResponse.Items[0];
            Cache?.AddUser(user);
            return user;
        }
    }

    public ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<string> usernames)
        => GetUsersAsync(usernames.TryGetReadOnlyMemory(out ReadOnlyMemory<string> usernamesMemory) ? usernamesMemory : usernames.ToArray(), ReadOnlyMemory<long>.Empty);

    public ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<long> userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> userIdsMemory) ? userIdsMemory : userIds.ToArray());

    public ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<string> usernames, IEnumerable<long> userIds)
    {
        bool usernamesIsMemory = usernames.TryGetReadOnlyMemory(out ReadOnlyMemory<string> usernamesMemory);
        bool userIdsIsMemory = userIds.TryGetReadOnlyMemory(out ReadOnlyMemory<long> userIdsMemory);

        return usernamesIsMemory switch
        {
            true when userIdsIsMemory => GetUsersAsync(usernamesMemory, userIdsMemory),
            true when !userIdsIsMemory => GetUsersAsync(usernamesMemory, userIds.ToArray()),
            false when userIdsIsMemory => GetUsersAsync(usernamesMemory.ToArray(), userIdsMemory),
            _ => GetUsersAsync(usernames.ToArray(), userIds.ToArray())
        };
    }

    public ValueTask<ImmutableArray<User>> GetUsersAsync(List<string> usernames)
        => GetUsersAsync(ListMarshal.AsMemory(usernames), ReadOnlyMemory<long>.Empty);

    public ValueTask<ImmutableArray<User>> GetUsersAsync(List<long> userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, ListMarshal.AsMemory(userIds));

    public ValueTask<ImmutableArray<User>> GetUsersAsync(List<string> usernames, List<long> userIds)
        => GetUsersAsync(ListMarshal.AsMemory(usernames), ListMarshal.AsMemory(userIds));

    public ValueTask<ImmutableArray<User>> GetUsersAsync(params string[] usernames)
        => GetUsersAsync(usernames, ReadOnlyMemory<long>.Empty);

    public ValueTask<ImmutableArray<User>> GetUsersAsync(params long[] userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds);

    public ValueTask<ImmutableArray<User>> GetUsersAsync(string[] usernames, long[] userIds)
        => GetUsersAsync(usernames.AsMemory(), userIds);

    public ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<string> usernames)
        => GetUsersAsync(usernames, ReadOnlyMemory<long>.Empty);

    public ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<long> userIds)
        => GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds);

    public async ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds)
    {
        using RentedArray<User> buffer = ArrayPool<User>.Shared.RentAsRentedArray(usernames.Length + userIds.Length);
        int userCount = await GetUsersAsync(usernames, userIds, RentedArrayMarshal.GetArray(buffer));
        return userCount == 0 ? [] : ImmutableCollectionsMarshal.AsImmutableArray(buffer.ToArray(..userCount));
    }

    public ValueTask<int> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds, User[] destination)
    {
        int parameterCount = usernames.Length + userIds.Length;
        return parameterCount switch
        {
            0 => ValueTask.FromResult(0),
            > 100 => throw new ArgumentException("The endpoint allows only up to 100 parameters. You can't pass more than 100 usernames or user ids in total."),
            _ => GetUsersCoreAsync(usernames, userIds, destination)
        };

        async ValueTask<int> GetUsersCoreAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds, User[] destination)
        {
            using UrlBuilder urlBuilder = new(ApiBaseUrl, UsersEndpoint, usernames.Length * 35 + userIds.Length * 25 + 50);
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
            HelixResponse<User> helixResponse = JsonSerializer.Deserialize(response.AsSpan(), HelixJsonSerializerContext.Default.HelixResponseUser);
            int deserializedUserCount = helixResponse.Items.Length;
            if (deserializedUserCount != 0)
            {
                helixResponse.Items.CopyTo(destination[cachedUserCount..]);
            }

            Cache?.AddUsers(destination.AsSpan(cachedUserCount..(cachedUserCount + deserializedUserCount)));
            return deserializedUserCount + cachedUserCount;
        }
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
