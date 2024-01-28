using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace HLE.Twitch.Bttv.Models;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{Name}\"")]
public sealed class Emote : CachedModel, IEquatable<Emote>
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("code")]
    public required string Name { get; init; }

    public bool Equals(Emote? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => obj is Emote other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Emote? left, Emote? right) => Equals(left, right);

    public static bool operator !=(Emote? left, Emote? right) => !(left == right);
}
