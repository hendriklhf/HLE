using System;
using System.Text.Json.Serialization;
using HLE.Twitch.Api.JsonConverters;

namespace HLE.Twitch.Api.Models;

public sealed class Emote : IEquatable<Emote>
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(Int64StringConverter))]
    public required long Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("images")]
    //TODO: needs converter
    public required string[] Images { get; init; }

    [JsonPropertyName("format")]
    //TODO: needs converter
    public required ImageFormatsFlag ImageFormatsFlags { get; init; }

    public bool Equals(Emote? other)
    {
        return ReferenceEquals(this, other) || Id == other?.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Emote other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Emote? left, Emote? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Emote? left, Emote? right)
    {
        return !(left == right);
    }
}
