using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Strings;

namespace HLE.Memory;

public unsafe ref struct UnsafeBufferWriter<T>(ref T buffer)
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

    public void Write(T item) => Unsafe.Add(ref _buffer, Count++) = item;

    public void Write(T[] items) => Write(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    public void Write(scoped Span<T> items) => Write(ref MemoryMarshal.GetReference(items), items.Length);

    public void Write(params scoped ReadOnlySpan<T> items) => Write(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(scoped ref T source, int count)
    {
        ref T destination = ref Unsafe.Add(ref _buffer, Count);
        SpanHelpers<T>.Memmove(ref destination, ref source, (uint)count);
        Count += count;
    }

    public override readonly string ToString()
    {
        if (typeof(T) != typeof(char))
        {
            return ToStringHelpers.FormatCollection(typeof(UnsafeBufferWriter<T>), Count);
        }

        if (Count == 0)
        {
            return string.Empty;
        }

        ReadOnlySpan<char> chars = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, char>(ref _buffer), Count);
        return new(chars);
    }

    public readonly bool Equals(UnsafeBufferWriter<T> other) => Count == other.Count && Unsafe.AreSame(ref _buffer, ref other._buffer);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    public override readonly int GetHashCode() => ((nuint)Unsafe.AsPointer(ref _buffer)).GetHashCode();

    public static bool operator ==(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => left.Equals(right);

    public static bool operator !=(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => !(left == right);
}
