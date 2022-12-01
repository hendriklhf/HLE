namespace HLE.Twitch.Models;

/// <summary>
/// A class that represent a badge of user.
/// </summary>
public sealed class Badge
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
}
