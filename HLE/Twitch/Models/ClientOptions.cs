using System;
using System.Diagnostics.Contracts;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Twitch.Models;

/// <summary>
/// Options for <see cref="TwitchClient"/>.
/// </summary>
public readonly struct ClientOptions : IBitwiseEquatable<ClientOptions>
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

    [Pure]
    public bool Equals(ClientOptions other) => StructMarshal.EqualsBitwise(this, other);

    [Pure]
    public override bool Equals(object? obj) => obj is ClientOptions other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(UseSSL, IsVerifiedBot, ParsingMode);

    public static bool operator ==(ClientOptions left, ClientOptions right) => left.Equals(right);

    public static bool operator !=(ClientOptions left, ClientOptions right) => !(left == right);
}
