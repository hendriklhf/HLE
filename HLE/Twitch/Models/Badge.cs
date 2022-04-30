namespace HLE.Twitch.Models;

public class Badge
{
    public string Name { get; }

    public int Level { get; }

    public Badge(string name, int lvl)
    {
        Name = name;
        Level = lvl;
    }
}
