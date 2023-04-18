﻿using System;
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
    public async ValueTask<User?> GetUserAsync(long userId)
    {
        if (TryGetUserFromCache(userId, out User? user))
        {
            return user;
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "users", _apiBaseUrl.Length + "users".Length + 50);
        urlBuilder.AppendParameter("id", userId);
        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetUsersResponse getUsersResponse = JsonSerializer.Deserialize<GetUsersResponse>(response.Bytes.Span);
        if (getUsersResponse.Users.Length == 0)
        {
            return null;
        }

        user = getUsersResponse.Users[0];
        Cache?.AddUser(user);
        return user;
    }

    public async ValueTask<User?> GetUserAsync(string username)
    {
        return await GetUserAsync(username.AsMemory());
    }

    public async ValueTask<User?> GetUserAsync(ReadOnlyMemory<char> username)
    {
        if (TryGetUserFromCache(username.Span, out User? user))
        {
            return user;
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "users", _apiBaseUrl.Length + "users".Length + 50);
        urlBuilder.AppendParameter("login", username.Span);
        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetUsersResponse getUsersResponse = JsonSerializer.Deserialize<GetUsersResponse>(response.Bytes.Span);
        if (getUsersResponse.Users.Length == 0)
        {
            return null;
        }

        user = getUsersResponse.Users[0];
        Cache?.AddUser(user);
        return user;
    }

    public async ValueTask<User[]> GetUsersAsync(IEnumerable<string> usernames)
    {
        return await GetUsersAsync(usernames.ToArray(), ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<User[]> GetUsersAsync(IEnumerable<long> userIds)
    {
        return await GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds.ToArray());
    }

    public async ValueTask<User[]> GetUsersAsync(IEnumerable<string> usernames, IEnumerable<long> userIds)
    {
        return await GetUsersAsync(usernames.ToArray(), userIds.ToArray());
    }

    public async ValueTask<User[]> GetUsersAsync(List<string> usernames)
    {
        return await GetUsersAsync(CollectionsMarshal.AsSpan(usernames).AsMemoryDangerous(), ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<User[]> GetUsersAsync(List<long> userIds)
    {
        return await GetUsersAsync(ReadOnlyMemory<string>.Empty, CollectionsMarshal.AsSpan(userIds).AsMemoryDangerous());
    }

    public async ValueTask<User[]> GetUsersAsync(List<string> usernames, List<long> userIds)
    {
        return await GetUsersAsync(CollectionsMarshal.AsSpan(usernames).AsMemoryDangerous(), CollectionsMarshal.AsSpan(userIds).AsMemoryDangerous());
    }

    public async ValueTask<User[]> GetUsersAsync(params string[] usernames)
    {
        return await GetUsersAsync(usernames, ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<User[]> GetUsersAsync(params long[] userIds)
    {
        return await GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds);
    }

    public async ValueTask<User[]> GetUsersAsync(string[] usernames, long[] userIds)
    {
        return await GetUsersAsync(usernames.AsMemory(), userIds);
    }

    public async ValueTask<User[]> GetUsersAsync(ReadOnlyMemory<string> usernames)
    {
        return await GetUsersAsync(usernames, ReadOnlyMemory<long>.Empty);
    }

    public async ValueTask<User[]> GetUsersAsync(ReadOnlyMemory<long> userIds)
    {
        return await GetUsersAsync(ReadOnlyMemory<string>.Empty, userIds);
    }

    public async ValueTask<User[]> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds)
    {
        using RentedArray<User> userBuffer = new(usernames.Length + userIds.Length);
        int userCount = await GetUsersAsync(usernames, userIds, userBuffer);
        return userCount == 0 ? Array.Empty<User>() : userBuffer[..userCount].ToArray();
    }

    public async ValueTask<int> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds, Memory<User> resultBuffer)
    {
        int parameterCount = usernames.Length + userIds.Length;
        switch (parameterCount)
        {
            case 0:
                return 0;
            case > 100:
                throw new ArgumentException("The endpoint allows only up to 100 parameters. You can't pass more than 100 usernames or user ids in total.");
        }

        using UrlBuilder urlBuilder = new(_apiBaseUrl, "users", usernames.Length * 35 + userIds.Length * 25 + 50);
        int cachedUserCount = 0;
        for (int i = 0; i < usernames.Length; i++)
        {
            string username = usernames.Span[i];
            if (TryGetUserFromCache(username, out User? user))
            {
                resultBuffer.Span[cachedUserCount++] = user;
                continue;
            }

            urlBuilder.AppendParameter("login", username);
        }

        for (int i = 0; i < userIds.Length; i++)
        {
            long userId = userIds.Span[i];
            if (TryGetUserFromCache(userId, out User? user))
            {
                resultBuffer.Span[cachedUserCount++] = user;
                continue;
            }

            urlBuilder.AppendParameter("id", userId);
        }

        if (urlBuilder.ParameterCount == 0)
        {
            return cachedUserCount;
        }

        using HttpResponse response = await ExecuteRequest(urlBuilder.ToString());
        GetUsersResponse getUsersResponse = JsonSerializer.Deserialize<GetUsersResponse>(response.Bytes.Span);
        int deserializedUserCount = getUsersResponse.Users.Length;
        if (deserializedUserCount > 0)
        {
            getUsersResponse.Users.CopyTo(resultBuffer.Span[cachedUserCount..]);
        }

        Cache?.AddUsers(resultBuffer.Span[cachedUserCount..(cachedUserCount + deserializedUserCount)]);
        return deserializedUserCount + cachedUserCount;
    }

    private bool TryGetUserFromCache(long userId, [MaybeNullWhen(false)] out User user)
    {
        if (Cache is not null)
        {
            return Cache.TryGetUser(userId, out user);
        }

        user = null;
        return false;
    }

    private bool TryGetUserFromCache(ReadOnlySpan<char> username, [MaybeNullWhen(false)] out User user)
    {
        if (Cache is not null)
        {
            return Cache.TryGetUser(username, out user);
        }

        user = null;
        return false;
    }
}
