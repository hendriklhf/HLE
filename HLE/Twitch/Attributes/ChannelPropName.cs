using System;

namespace HLE.Twitch.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal class ChannelPropName : Attribute
{
    public string Value { get; }

    public ChannelPropName(string value)
    {
        Value = value;
    }
}
