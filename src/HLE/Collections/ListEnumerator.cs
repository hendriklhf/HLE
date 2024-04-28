using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Marshalling;

namespace HLE.Collections;

public struct ListEnumerator<T>(List<T> list) :
    IEnumerator<T>,
    IEquatable<ListEnumerator<T>>
{
    public readonly T Current => Unsafe.Add(ref ListMarshal.GetReference(_list), _current);

    readonly object? IEnumerator.Current => Current;

    private readonly List<T> _list = list;
    private int _current = -1;

    public bool MoveNext() => ++_current < _list.Count;

    public void Reset() => _current = -1;

    public readonly void Dispose()
    {
    }

    [Pure]
    public readonly bool Equals(ListEnumerator<T> other) => _current == other._current && _list == other._list;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is ListEnumerator<T> other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(_list, _current);

    public static bool operator ==(ListEnumerator<T> left, ListEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(ListEnumerator<T> left, ListEnumerator<T> right) => !(left == right);
}
