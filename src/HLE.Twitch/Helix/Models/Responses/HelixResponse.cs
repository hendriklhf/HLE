using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace HLE.Twitch.Helix.Models.Responses;

public readonly struct HelixResponse<T> : IEquatable<HelixResponse<T>>
{
    [JsonPropertyName("data")]
    public required ImmutableArray<T> Items { get; init; } = [];

    public HelixResponse()
    {
    }

    public bool Equals(HelixResponse<T> other) => Items == other.Items;

    public override bool Equals(object? obj) => obj is HelixResponse<T> other && Equals(other);

    public override int GetHashCode() => Items.GetHashCode();

    public static bool operator ==(HelixResponse<T> left, HelixResponse<T> right) => left.Equals(right);

    public static bool operator !=(HelixResponse<T> left, HelixResponse<T> right) => !(left == right);
}
