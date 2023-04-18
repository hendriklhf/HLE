using System;
using System.Diagnostics.CodeAnalysis;
using HLE.Collections;
using HLE.Twitch.Api.Models;

namespace HLE.Twitch.Api;

public sealed class TwitchApiCache
{
    public CacheOptions Options { get; set; }

    private readonly DoubleDictionary<long, int, User> _userCache = new();

    private readonly DoubleDictionary<long, int, Stream> _streamCache = new();

    public TwitchApiCache(CacheOptions options)
    {
        Options = options;
    }

    public void AddUser(User user)
    {
        int usernameHash = string.GetHashCode(user.Username, StringComparison.OrdinalIgnoreCase);
        if (!_userCache.TryAdd(user.Id, usernameHash, user))
        {
            _userCache[user.Id, usernameHash] = user;
        }
    }

    public void AddUsers(ReadOnlySpan<User> users)
    {
        for (int i = 0; i < users.Length; i++)
        {
            AddUser(users[i]);
        }
    }

    public bool TryGetUser(long userId, [MaybeNullWhen(false)] out User user)
    {
        return _userCache.TryGetValue(userId, out user) && user.IsValid(Options.UserCacheTime);
    }

    public bool TryGetUser(ReadOnlySpan<char> username, [MaybeNullWhen(false)] out User user)
    {
        int usernameHash = string.GetHashCode(username, StringComparison.OrdinalIgnoreCase);
        return _userCache.TryGetValue(usernameHash, out user) && user.IsValid(Options.UserCacheTime);
    }

    public void AddStream(Stream stream)
    {
        int usernameHash = string.GetHashCode(stream.Username, StringComparison.OrdinalIgnoreCase);
        if (!_streamCache.TryAdd(stream.UserId, usernameHash, stream))
        {
            _streamCache[stream.UserId, usernameHash] = stream;
        }
    }

    public void AddStreams(ReadOnlySpan<Stream> streams)
    {
        for (int i = 0; i < streams.Length; i++)
        {
            AddStream(streams[i]);
        }
    }

    public bool TryGetStream(long userId, [MaybeNullWhen(false)] out Stream stream)
    {
        return _streamCache.TryGetValue(userId, out stream) && stream.IsValid(Options.StreamCacheTime);
    }

    public bool TryGetStream(ReadOnlySpan<char> username, [MaybeNullWhen(false)] out Stream stream)
    {
        int usernameHash = string.GetHashCode(username, StringComparison.OrdinalIgnoreCase);
        return _streamCache.TryGetValue(usernameHash, out stream) && stream.IsValid(Options.StreamCacheTime);
    }
}
