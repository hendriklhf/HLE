using System;
using System.Diagnostics.Contracts;

namespace HLE.SourceGenerators.Emojis;

public readonly struct EmojiModel(string name, string value) : IEquatable<EmojiModel>
{
    public string Name { get; } = name;

    public string Value { get; } = value;

    [Pure]
    public bool Equals(EmojiModel other) => Value == other.Value && Name == other.Name;

    [Pure]
    public override bool Equals(object? obj) => obj is EmojiModel other && Equals(other);

    [Pure]
    public override int GetHashCode() => Value.GetHashCode() ^ Name.GetHashCode();

    public static bool operator ==(EmojiModel left, EmojiModel right) => left.Equals(right);

    public static bool operator !=(EmojiModel left, EmojiModel right) => !(left == right);
}
