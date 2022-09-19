using System;

namespace HLE.Twitch.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class IrcTag : Attribute
{
    public string Name { get; }

    public IrcTag(string name)
    {
        Name = name;
    }
}
