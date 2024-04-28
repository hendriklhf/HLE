using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public sealed class ReplaceEnumerable<T>(IEnumerable<T> enumerable, Func<T, bool> predicate, T replacement) :
    IEnumerable<T>,
    IEquatable<ReplaceEnumerable<T>>
{
    private readonly IEnumerable<T> _enumerable = enumerable;
    private readonly Func<T, bool> _predicate = predicate;
    private readonly T _replacement = replacement;

    [Pure]
    public ReplaceEnumerator<T> GetEnumerator() => new(_enumerable.GetEnumerator(), _predicate, _replacement);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] ReplaceEnumerable<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ReplaceEnumerable<T>? left, ReplaceEnumerable<T>? right) => Equals(left, right);

    public static bool operator !=(ReplaceEnumerable<T>? left, ReplaceEnumerable<T>? right) => !(left == right);
}
