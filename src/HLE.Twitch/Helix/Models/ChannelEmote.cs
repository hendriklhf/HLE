using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;
using HLE.Twitch.JsonConverters;

namespace HLE.Twitch.Helix.Models;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{Name}\"")]
public sealed class ChannelEmote : Emote, IEquatable<ChannelEmote>
{
    [JsonPropertyName("emote_type")]
    [JsonConverter(typeof(EmoteTypeJsonConverter))]
    public required EmoteType Type { get; init; }

    [JsonPropertyName("emote_set_id")]
    public required string SetId { get; init; }

    [JsonPropertyName("tier")]
    [JsonConverter(typeof(EmoteTierJsonConverter))]
    public required EmoteTier Tier { get; init; }

    [Pure]
    public bool Equals(ChannelEmote? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => obj is ChannelEmote other && Equals(other);

    [Pure]
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(ChannelEmote? left, ChannelEmote? right) => Equals(left, right);

    public static bool operator !=(ChannelEmote? left, ChannelEmote? right) => !(left == right);
}
