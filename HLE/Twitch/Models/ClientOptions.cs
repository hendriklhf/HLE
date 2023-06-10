using System;
using HLE.Memory;

namespace HLE.Twitch.Models;

/// <summary>
/// Options for <see cref="TwitchClient"/>.
/// </summary>
public readonly struct ClientOptions : IEquatable<ClientOptions>
{
    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL { get; init; } = false;

    /// <summary>
    /// Indicates whether the bot is verified or not. If your bot is verified you can set this to true. Verified bots have higher rate limits.
    /// </summary>
    public bool IsVerifiedBot { get; init; } = false;

    /// <summary>
    /// The parsing mode of the IRC message parsers.
    /// </summary>
    public ParsingMode ParsingMode { get; init; } = ParsingMode.Balanced;

    public static ClientOptions Default => new();

    public ClientOptions()
    {
    }

    public bool Equals(ClientOptions other)
    {
        return MemoryHelper.EqualsBytes(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is ClientOptions other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UseSSL, IsVerifiedBot, ParsingMode);
    }

    public static bool operator ==(ClientOptions left, ClientOptions right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ClientOptions left, ClientOptions right)
    {
        return !(left == right);
    }
}
