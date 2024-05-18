using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Current: {Current}")]
public struct ArrayEnumerator<T> : IEnumerator<T>, IEquatable<ArrayEnumerator<T>>
{
    public readonly T Current => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_array), _current);

    readonly object? IEnumerator.Current => Current;

    private readonly T[] _array;
    private int _current;
    private readonly int _end;

    public ArrayEnumerator(T[] array)
    {
        _array = array;
        _current = -1;
        _end = array.Length - 1;
    }

    public ArrayEnumerator(T[] array, int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)(uint)start + (uint)length, (uint)array.Length);

        _array = array;
        _current = start - 1;
        _end = _current + length;
    }

    public ArrayEnumerator(List<T> list)
    {
        _array = ListMarshal.GetArray(list);
        _current = -1;
        _end = list.Count - 1;
    }

    public bool MoveNext() => _current++ < _end;

    [DoesNotReturn]
    public void Reset() => throw new NotSupportedException();

    [Pure]
    public readonly bool Equals(ArrayEnumerator<T> other)
        => _array == other._array && _current == other._current && _end == other._end;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(_array, _current, _end);

    public static bool operator ==(ArrayEnumerator<T> left, ArrayEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(ArrayEnumerator<T> left, ArrayEnumerator<T> right) => !(left == right);

    readonly void IDisposable.Dispose()
    {
    }
}
