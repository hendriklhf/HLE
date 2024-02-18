using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HLE.Twitch.Helix.Models;

namespace HLE.Twitch.Helix;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface ITwitchUserApi
{
    ValueTask<User?> GetUserAsync(long userId);

    ValueTask<User?> GetUserAsync(string username);

    ValueTask<User?> GetUserAsync(ReadOnlyMemory<char> username);

    ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<string> usernames);

    ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<long> userIds);

    ValueTask<ImmutableArray<User>> GetUsersAsync(IEnumerable<string> usernames, IEnumerable<long> userIds);

    ValueTask<ImmutableArray<User>> GetUsersAsync(List<string> usernames);

    ValueTask<ImmutableArray<User>> GetUsersAsync(List<long> userIds);

    ValueTask<ImmutableArray<User>> GetUsersAsync(List<string> usernames, List<long> userIds);

    ValueTask<ImmutableArray<User>> GetUsersAsync(params string[] usernames);

    ValueTask<ImmutableArray<User>> GetUsersAsync(params long[] userIds);

    ValueTask<ImmutableArray<User>> GetUsersAsync(string[] usernames, long[] userIds);

    ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<string> usernames);

    ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<long> userIds);

    ValueTask<ImmutableArray<User>> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds);

    ValueTask<int> GetUsersAsync(ReadOnlyMemory<string> usernames, ReadOnlyMemory<long> userIds, User[] destination);
}
