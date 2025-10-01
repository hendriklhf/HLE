using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.RemoteExecution;

public sealed class RemoteExecutorResult : IEquatable<RemoteExecutorResult>
{
    public required int ExitCode { get; init; }

    public string? Output { get; init; }

    [Pure]
    public bool Equals([NotNullWhen(true)] RemoteExecutorResult? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RemoteExecutorResult? left, RemoteExecutorResult? right) => Equals(left, right);

    public static bool operator !=(RemoteExecutorResult? left, RemoteExecutorResult? right) => !(left == right);
}
