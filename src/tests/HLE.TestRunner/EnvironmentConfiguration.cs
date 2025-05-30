using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.TestRunner;

internal sealed class EnvironmentConfiguration(string targetFramework, string configuration, string runtimeIdentifier) : IEquatable<EnvironmentConfiguration>
{
    public string TargetFramework { get; } = targetFramework;

    public string Configuration { get; } = configuration;

    public string RuntimeIdentifier { get; } = runtimeIdentifier;

    public IReadOnlyDictionary<string, string> EnvironmentVariables => _environmentVariables;

    private readonly Dictionary<string, string> _environmentVariables = new();

    public void AddEnvironmentVariable(string name, string value) => _environmentVariables.Add(name, value);

    [Pure]
    public bool Equals([NotNullWhen(true)] EnvironmentConfiguration? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(EnvironmentConfiguration? left, EnvironmentConfiguration? right) => Equals(left, right);

    public static bool operator !=(EnvironmentConfiguration? left, EnvironmentConfiguration? right) => !(left == right);
}
