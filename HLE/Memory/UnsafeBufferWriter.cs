using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Strings;

namespace HLE.Memory;

public unsafe ref struct UnsafeBufferWriter<T>(ref T buffer)
{
    public readonly Span<T> WrittenSpan => MemoryMarshal.CreateSpan(ref _buffer, Count);

    public int Count { get; private set; }

    private readonly ref T _buffer = ref buffer;

    public UnsafeBufferWriter(Span<T> buffer) : this(ref MemoryMarshal.GetReference(buffer))
    {
    }

    public UnsafeBufferWriter(T[] buffer) : this(ref MemoryMarshal.GetArrayDataReference(buffer))
    {
    }

    public void Write(T item) => Unsafe.Add(ref _buffer, Count++) = item;

    public void Write(T[] items)
        => Write(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    public void Write(Span<T> items)
        => Write(ref MemoryMarshal.GetReference(items), items.Length);

    public void Write(ReadOnlySpan<T> items)
        => Write(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ref T source, int count)
    {
        ref T destination = ref Unsafe.Add(ref _buffer, Count);
        CopyWorker<T>.Copy(ref source, ref destination, (uint)count);
        Count += count;
    }

    // ReSharper disable once ArrangeModifiersOrder
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

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => ((nuint)Unsafe.AsPointer(ref _buffer)).GetHashCode();

    public static bool operator ==(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => left.Equals(right);

    public static bool operator !=(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => !(left == right);
}
