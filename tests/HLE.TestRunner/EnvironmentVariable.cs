using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.TestRunner;

internal sealed class EnvironmentVariable(string name, ImmutableArray<string> values, bool isApplicable) : IEquatable<EnvironmentVariable>
{
    public string Name { get; } = name;

    public ImmutableArray<string> Values { get; } = values;

    public bool IsApplicable { get; } = isApplicable;

    [Pure]
    public bool Equals([NotNullWhen(true)] EnvironmentVariable? other) => Name == other?.Name && Values.Equals(other.Values);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is EnvironmentVariable other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Name, Values);

    public static bool operator ==(EnvironmentVariable? left, EnvironmentVariable? right) => Equals(left, right);

    public static bool operator !=(EnvironmentVariable? left, EnvironmentVariable? right) => !(left == right);
}
