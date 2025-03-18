﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Twitch.Tmi.Models;

/// <summary>
/// Options for <see cref="TwitchClient"/>.
/// </summary>
public readonly struct ClientOptions : IBitwiseEquatable<ClientOptions>
{
    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    public bool UseSsl { get; init; } = true;

    /// <summary>
    /// Indicates whether the bot is verified or not. If your bot is verified you can set this to true. Verified bots have higher rate limits.
    /// </summary>
    public bool IsVerifiedBot { get; init; }

    public static ClientOptions Default => new();

    public ClientOptions()
    {
    }

    [Pure]
    public bool Equals(ClientOptions other) => StructMarshal.EqualsBitwise(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ClientOptions other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(UseSsl, IsVerifiedBot);

    public static bool operator ==(ClientOptions left, ClientOptions right) => left.Equals(right);

    public static bool operator !=(ClientOptions left, ClientOptions right) => !(left == right);
}
