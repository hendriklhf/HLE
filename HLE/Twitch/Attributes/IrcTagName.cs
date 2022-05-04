using System;

namespace HLE.Twitch.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal class IrcTagName : Attribute
{
    public string Value { get; }

    public IrcTagName(string value)
    {
        Value = value;
    }
}
