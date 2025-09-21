using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public sealed class ReplaceEnumerable<T>(IEnumerable<T> enumerable, Func<T, bool> predicate, T replacement) :
    IEnumerable<T>,
    IEnumerator<T>,
    IEquatable<ReplaceEnumerable<T>>
{
    private readonly IEnumerator<T> _enumerator = enumerable.GetEnumerator();
    private readonly Func<T, bool> _predicate = predicate;
    private readonly T _replacement = replacement;

    public T Current => GetCurrent();

    object? IEnumerator.Current => Current;

    private T GetCurrent()
    {
        T current = _enumerator.Current;
        return _predicate(current) ? _replacement : current;
    }

    public bool MoveNext() => _enumerator.MoveNext();

    public void Reset() => _enumerator.Reset();

    public void Dispose() => _enumerator.Dispose();

    [Pure]
    public ReplaceEnumerable<T> GetEnumerator() => this;

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
