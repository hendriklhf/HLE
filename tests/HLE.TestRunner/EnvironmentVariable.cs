using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.TestRunner;

internal readonly struct EnvironmentVariable(string name, ImmutableArray<string> values) : IEquatable<EnvironmentVariable>
{
    public string Name { get; } = name;

    public ImmutableArray<string> Values { get; } = values;

    [Pure]
    public bool Equals(EnvironmentVariable other) => Name == other.Name && Values.Equals(other.Values);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is EnvironmentVariable other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Name, Values);

    public static bool operator ==(EnvironmentVariable left, EnvironmentVariable right) => left.Equals(right);

    public static bool operator !=(EnvironmentVariable left, EnvironmentVariable right) => !(left == right);
}
