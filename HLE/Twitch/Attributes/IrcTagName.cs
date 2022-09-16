using System;

namespace HLE.Twitch.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class IrcTagName : Attribute
{
    public string Value { get; }

    public IrcTagName(string value)
    {
        Value = value;
    }
}
