using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public ref struct MemoryEnumerator<T> :
#if NET9_0_OR_GREATER
    IEquatable<MemoryEnumerator<T>>,
#endif
    IEnumerator<T>
{
    public readonly ref T Current => ref _current;

    readonly T IEnumerator<T>.Current => _current;

    readonly object? IEnumerator.Current => Current;

    private ref T _current;
    private readonly ref T _end;

    public static MemoryEnumerator<T> Empty => default;

    public MemoryEnumerator(ref T memory, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _current = ref Unsafe.Add(ref memory, -1);
        _end = ref Unsafe.Add(ref memory, length);
    }

    public MemoryEnumerator(T[] items)
    {
        _current = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(items), -1);
        _end = ref Unsafe.Add(ref _current, items.Length);
    }

    public MemoryEnumerator(Span<T> items)
    {
        _current = ref Unsafe.Add(ref MemoryMarshal.GetReference(items), -1);
        _end = ref Unsafe.Add(ref _current, items.Length);
    }

    public MemoryEnumerator(ReadOnlySpan<T> items)
    {
        _current = ref Unsafe.Add(ref MemoryMarshal.GetReference(items), -1);
        _end = ref Unsafe.Add(ref _current, items.Length);
    }

    public bool MoveNext()
    {
        if (Unsafe.AreSame(ref _current, ref _end))
        {
            return false;
        }

        _current = ref Unsafe.Add(ref _current, 1);
        return true;
    }

    [DoesNotReturn]
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
