using System;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represent a badge or badge info of a user.
/// </summary>
public readonly struct Badge : IEquatable<Badge>
{
    /// <summary>
    /// The name of the badge.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The level of the badge.
    /// </summary>
    public string Level { get; }

    /// <summary>
    /// The default constructor of <see cref="Badge"/>.
    /// </summary>
    /// <param name="name">The name of the badge.</param>
    /// <param name="level">The level of the badge.</param>
    public Badge(string name, string level)
    {
        Name = name;
        Level = level;
    }

    public bool Equals(Badge other)
    {
        return Name == other.Name && Level == other.Level;
    }

    public override bool Equals(object? obj)
    {
        return obj is Badge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Level);
    }

    public static bool operator ==(Badge left, Badge right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Badge left, Badge right)
    {
        return !(left == right);
    }
}
