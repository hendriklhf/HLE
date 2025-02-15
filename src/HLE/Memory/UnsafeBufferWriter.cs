using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

public ref struct UnsafeBufferWriter<T>(ref T buffer) :
    IBufferWriter<T>,
    IEquatable<UnsafeBufferWriter<T>>
{
    public readonly Span<T> WrittenSpan => MemoryMarshal.CreateSpan(ref _buffer, Count);

    public int Count { get; private set; }

    private readonly ref T _buffer = ref buffer;

    public UnsafeBufferWriter(List<T> list) : this(ref ListMarshal.GetReference(list))
    {
    }

    public UnsafeBufferWriter(T[] buffer) : this(ref MemoryMarshal.GetArrayDataReference(buffer))
    {
    }

    public UnsafeBufferWriter(Span<T> buffer) : this(ref MemoryMarshal.GetReference(buffer))
    {
    }

    public void Advance(int count) => Count += count;

    public readonly ref T GetReference() => ref Unsafe.Add(ref _buffer, Count);

    public readonly Span<T> GetSpan(int sizeHint = 0) => MemoryMarshal.CreateSpan(ref GetReference(), sizeHint);

    public readonly Memory<T> GetMemory(int sizeHint = 0) => throw new NotSupportedException();

    public void Write(T item) => Unsafe.Add(ref _buffer, Count++) = item;

    public void Write(List<T> items) => Write(ref ListMarshal.GetReference(items), items.Count);

    public void Write(T[] items) => Write(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    public void Write(scoped Span<T> items) => Write(ref MemoryMarshal.GetReference(items), items.Length);

    public void Write(params scoped ReadOnlySpan<T> items) => Write(ref MemoryMarshal.GetReference(items), items.Length);

    public void Write(scoped ref T source, int count)
    {
        ref T destination = ref Unsafe.Add(ref _buffer, Count);
        SpanHelpers.Memmove(ref destination, ref source, count);
        Count += count;
    }

    [Pure]
    public override readonly string ToString()
    {
        if (typeof(T) != typeof(char))
        {
            return ToStringHelpers.FormatCollection<UnsafeBufferWriter<T>>(Count);
        }

        if (Count == 0)
        {
            return string.Empty;
        }

        Span<char> chars = Unsafe.BitCast<Span<T>, Span<char>>(WrittenSpan);
        return new(chars);
    }

    [Pure]
    public readonly bool Equals(UnsafeBufferWriter<T> other) => Count == other.Count && Unsafe.AreSame(ref _buffer, ref other._buffer);

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => Count;

    public static bool operator ==(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => left.Equals(right);

    public static bool operator !=(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => !(left == right);
}
