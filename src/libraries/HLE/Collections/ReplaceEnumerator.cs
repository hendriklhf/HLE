using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Collections;

[StructLayout(LayoutKind.Auto)]
public readonly struct ReplaceEnumerator<T>(IEnumerator<T> enumerator, Func<T, bool> predicate, T replacement) :
    IEnumerator<T>,
    IEquatable<ReplaceEnumerator<T>>
{
    public T Current => GetCurrent();

    object? IEnumerator.Current => Current;

    private readonly IEnumerator<T> _enumerator = enumerator;
    private readonly Func<T, bool> _predicate = predicate;
    private readonly T _replacement = replacement;

    public static ReplaceEnumerator<T> Empty => new(EmptyEnumeratorCache<T>.Enumerator, null!, default!);

    public bool MoveNext() => _enumerator.MoveNext();

    private T GetCurrent()
    {
        T current = _enumerator.Current;
        return _predicate(current) ? _replacement : current;
    }

    public void Reset() => _enumerator.Reset();

    public void Dispose() => _enumerator.Dispose();

    [Pure]
    public bool Equals(ReplaceEnumerator<T> other) =>
        ReferenceEquals(_enumerator, other._enumerator) &&
        ReferenceEquals(_predicate, other._predicate) &&
        _replacement?.Equals(other._replacement) == true;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ReplaceEnumerator<T> other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(_enumerator, _predicate, _replacement);

    public static bool operator ==(ReplaceEnumerator<T> left, ReplaceEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(ReplaceEnumerator<T> left, ReplaceEnumerator<T> right) => !(left == right);
}
