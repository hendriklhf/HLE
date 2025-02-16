using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.TestRunner;

internal sealed class UnitTestRunResult : IEquatable<UnitTestRunResult>
{
    public bool IsSuccess { get; }

    public static UnitTestRunResult Success { get; } = new(true);

    public static UnitTestRunResult Failure { get; } = new(false);

    private UnitTestRunResult(bool success) => IsSuccess = success;

    [Pure]
    public bool Equals([NotNullWhen(true)] UnitTestRunResult? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(UnitTestRunResult? left, UnitTestRunResult? right) => Equals(left, right);

    public static bool operator !=(UnitTestRunResult? left, UnitTestRunResult? right) => !(left == right);
}
