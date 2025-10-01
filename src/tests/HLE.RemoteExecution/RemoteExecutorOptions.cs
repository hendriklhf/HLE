using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.RemoteExecution;

public sealed class RemoteExecutorOptions : IEquatable<RemoteExecutorOptions>
{
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    [Pure]
    public bool Equals([NotNullWhen(true)] RemoteExecutorOptions? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RemoteExecutorOptions? left, RemoteExecutorOptions? right) => Equals(left, right);

    public static bool operator !=(RemoteExecutorOptions? left, RemoteExecutorOptions? right) => !(left == right);
}
