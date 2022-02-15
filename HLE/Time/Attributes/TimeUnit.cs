using System;

namespace HLE.Time.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class TimeUnit : Attribute
{
    public string Value { get; }

    public TimeUnit(string value)
    {
        Value = value;
    }
}
