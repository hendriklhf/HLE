using System;
using System.Diagnostics.CodeAnalysis;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Represents a badge or badge info of a user.
/// </summary>
/// <param name="name">The name of the badge.</param>
/// <param name="level">The level of the badge.</param>
public readonly struct Badge(string name, string level) : IEquatable<Badge>
{
    /// <summary>
    /// The name of the badge.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The level of the badge.
    /// </summary>
    public string Level { get; } = level;

    public bool Equals(Badge other) => Name == other.Name && Level == other.Level;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Badge other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Name, Level);

    public static bool operator ==(Badge left, Badge right) => left.Equals(right);

    public static bool operator !=(Badge left, Badge right) => !(left == right);
}
