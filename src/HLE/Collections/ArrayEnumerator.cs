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
    private readonly int _length;

    public static ArrayEnumerator<T> Empty => default;

    public ArrayEnumerator(T[] array)
    {
        _array = array;
        _current = -1;
        _length = array.Length;
    }

    public ArrayEnumerator(T[] array, int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, array.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)(uint)start + (uint)length, (uint)array.Length);

        _array = array;
        _current = start - 1;
        _length = _current + length;
    }

    public ArrayEnumerator(List<T> list)
    {
        _array = ListMarshal.GetArray(list);
        _current = -1;
        _length = list.Count;
    }

    public bool MoveNext() => ++_current < _length;

    [DoesNotReturn]
    readonly void IEnumerator.Reset() => throw new NotSupportedException();

    readonly void IDisposable.Dispose()
    {
    }

    [Pure]
    public readonly bool Equals(ArrayEnumerator<T> other)
        => _array == other._array && _current == other._current && _length == other._length;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(_array, _current, _length);

    public static bool operator ==(ArrayEnumerator<T> left, ArrayEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(ArrayEnumerator<T> left, ArrayEnumerator<T> right) => !(left == right);
}
