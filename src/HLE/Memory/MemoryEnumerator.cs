using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public ref struct MemoryEnumerator<T> : IEnumerator<T>, IEquatable<MemoryEnumerator<T>>
{
    public readonly T Current => _current;

    readonly object? IEnumerator.Current => Current;

    private ref T _current;
    private readonly ref T _end;

    public static MemoryEnumerator<T> Empty => new(ref Unsafe.NullRef<T>(), 0);

    public MemoryEnumerator(ref T memory, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _current = ref memory;
        _end = ref Unsafe.Add(ref memory, length);
    }

    public MemoryEnumerator(T[] items)
    {
        ref T current = ref MemoryMarshal.GetArrayDataReference(items);
        _current = ref Unsafe.Add(ref current, -1);
        _end = ref Unsafe.Add(ref current, items.Length);
    }

    public MemoryEnumerator(Span<T> items)
    {
        ref T current = ref MemoryMarshal.GetReference(items);
        _current = ref Unsafe.Add(ref current, -1);
        _end = ref Unsafe.Add(ref current, items.Length);
    }

    public MemoryEnumerator(ReadOnlySpan<T> items)
    {
        ref T current = ref MemoryMarshal.GetReference(items);
        _current = ref Unsafe.Add(ref current, -1);
        _end = ref Unsafe.Add(ref current, items.Length);
    }

    public bool MoveNext()
    {
        _current = ref Unsafe.Add(ref _current, 1);
        return !Unsafe.AreSame(ref _current, ref _end);
    }

    void IEnumerator.Reset() => throw new NotSupportedException();

    readonly void IDisposable.Dispose()
    {
    }

    [Pure]
    public readonly bool Equals(scoped MemoryEnumerator<T> other)
        => Unsafe.AreSame(ref _current, ref other._current) &&
           Unsafe.AreSame(ref _end, ref other._end);

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly unsafe int GetHashCode() => HashCode.Combine((nuint)Unsafe.AsPointer(ref _current), (nuint)Unsafe.AsPointer(ref _end));

    public static bool operator ==(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => !(left == right);
}
