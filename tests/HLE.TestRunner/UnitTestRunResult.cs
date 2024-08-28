using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.TestRunner;

internal sealed class UnitTestRunResult(bool success) : IEquatable<UnitTestRunResult>
{
    public bool IsSuccess { get; } = success;

    public static UnitTestRunResult Success { get; } = new(true);

    public static UnitTestRunResult Failure { get; } = new(false);

    [Pure]
    public bool Equals([NotNullWhen(true)] UnitTestRunResult? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(UnitTestRunResult? left, UnitTestRunResult? right) => Equals(left, right);

    public static bool operator !=(UnitTestRunResult? left, UnitTestRunResult? right) => !(left == right);
}
