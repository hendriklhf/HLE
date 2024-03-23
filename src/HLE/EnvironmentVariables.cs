using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using HLE.Collections;

namespace HLE;

public readonly struct EnvironmentVariables : IReadOnlyDictionary<string, string>, ICountable, IEquatable<EnvironmentVariables>
{
    public string? this[string name] => _environmentVariables.GetValueOrDefault(name);

    string IReadOnlyDictionary<string, string>.this[string key] => _environmentVariables[key];

    public ImmutableArray<string> Names => _environmentVariables.Keys;

    public ImmutableArray<string> Values => _environmentVariables.Values;

    IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => Names;

    IEnumerable<string> IReadOnlyDictionary<string, string>.Values => Values;

    public int Count => _environmentVariables.Count;

    private readonly FrozenDictionary<string, string> _environmentVariables = FrozenDictionary<string, string>.Empty;

    public static EnvironmentVariables Empty => new();

    public EnvironmentVariables()
    {
    }

    public EnvironmentVariables(FrozenDictionary<string, string> environmentVariables) => _environmentVariables = environmentVariables;

    public static EnvironmentVariables Create()
    {
        IEnvironmentVariableProvider provider = EnvironmentVariableProvider.Create();
        return provider.GetEnvironmentVariables();
    }

    bool IReadOnlyDictionary<string, string>.ContainsKey(string key) => _environmentVariables.ContainsKey(key);

    bool IReadOnlyDictionary<string, string>.TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => _environmentVariables.TryGetValue(key, out value);

    public FrozenDictionary<string, string>.Enumerator GetEnumerator() => _environmentVariables.GetEnumerator();

    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(EnvironmentVariables other) => ReferenceEquals(_environmentVariables, other._environmentVariables);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is EnvironmentVariables other && Equals(other);

    [Pure]
    public override int GetHashCode() => _environmentVariables.GetHashCode();

    public static bool operator ==(EnvironmentVariables left, EnvironmentVariables right) => left.Equals(right);

    public static bool operator !=(EnvironmentVariables left, EnvironmentVariables right) => !(left == right);
}
