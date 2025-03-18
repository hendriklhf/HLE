using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE;

public readonly struct EnvironmentVariable(string name, string value) : IEquatable<EnvironmentVariable>
{
    public string Name { get; } = name;

    public string Value { get; } = value;

    public EnvironmentVariable(KeyValuePair<string, string> nameValuePair)
        : this(nameValuePair.Key, nameValuePair.Value)
    {
    }

    [Pure]
    public override string ToString() => $"{Name}=\"{Value}\"";

    [Pure]
    public bool Equals(EnvironmentVariable other) => Name == other.Name && Value == other.Value;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is EnvironmentVariable other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Name, Value);

    public static bool operator ==(EnvironmentVariable left, EnvironmentVariable right) => left.Equals(right);

    public static bool operator !=(EnvironmentVariable left, EnvironmentVariable right) => !(left == right);
}
