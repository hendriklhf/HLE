using System;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace HLE.SourceGenerators.Emojis;

public struct Emoji : IEquatable<Emoji>
{
    [JsonPropertyName("emoji")]
    public string Value { get; set; }

    public readonly string Name => Aliases[0];

    [JsonPropertyName("aliases")]
    public string[] Aliases { get; set; }

    [Pure]
    public readonly bool Equals(Emoji other) => Value == other.Value && Name == other.Name;

    [Pure]
    public override readonly bool Equals(object? obj) => obj is Emoji other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => Value.GetHashCode() ^ Name.GetHashCode();

    public static bool operator ==(Emoji left, Emoji right) => left.Equals(right);

    public static bool operator !=(Emoji left, Emoji right) => !(left == right);
}
