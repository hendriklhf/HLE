using System;

namespace HLE.Twitch.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MsgPropName : Attribute
{
    public string Value { get; }

    public MsgPropName(string value)
    {
        Value = value;
    }
}
