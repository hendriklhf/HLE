namespace HLE.Twitch.Models;

/// <summary>
/// A class that represent a badge of user.
/// </summary>
public class Badge
{
    /// <summary>
    /// The name of the badge.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The level of the badge.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// The basic constructor of <see cref="Badge"/>.
    /// </summary>
    /// <param name="name">The name of the badge.</param>
    /// <param name="lvl">The level of the badge.</param>
    public Badge(string name, int lvl)
    {
        Name = name;
        Level = lvl;
    }
}
