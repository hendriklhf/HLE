using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

[StructLayout(LayoutKind.Auto)]
public ref struct UnsafeBufferWriter<T>(ref T buffer) :
#if NET9_0_OR_GREATER
    IEquatable<UnsafeBufferWriter<T>>,
#endif
    IBufferWriter<T>
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

    public readonly Span<T> GetSpan(int sizeHint)
    {
        Debug.Assert(sizeHint >= 0);
        return MemoryMarshal.CreateSpan(ref GetReference(), sizeHint);
    }

    readonly Memory<T> IBufferWriter<T>.GetMemory(int sizeHint) => throw new NotSupportedException();

    public void Write(T item) => Unsafe.Add(ref _buffer, Count++) = item;

    public void Write(List<T> items) => Write(ref ListMarshal.GetReference(items), items.Count);

    public void Write(T[] items) => Write(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    public void Write(scoped Span<T> items) => Write(ref MemoryMarshal.GetReference(items), items.Length);

    public void Write(params scoped ReadOnlySpan<T> items) => Write(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(scoped ref T source, int count)
    {
        ref T destination = ref Unsafe.Add(ref _buffer, Count);
        SpanHelpers.Memmove(ref destination, ref source, count);
        Count += count;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SpanHelpers.Clear(ref _buffer, Count);
        }

        Count = 0;
    }

    public readonly MemoryEnumerator<T> GetEnumerator() => new(WrittenSpan);

    [Pure]
    public override readonly string ToString()
    {
        if (typeof(T) != typeof(char))
        {
#if NET9_0_OR_GREATER
            return ToStringHelpers.FormatCollection<UnsafeBufferWriter<T>>(Count);
#else
            return ToStringHelpers.FormatCollection(typeof(UnsafeBufferWriter<T>), Count);
#endif
        }

        if (Count == 0)
        {
            return string.Empty;
        }

#if NET9_0_OR_GREATER
        ReadOnlySpan<char> chars = Unsafe.BitCast<Span<T>, ReadOnlySpan<char>>(WrittenSpan);
#else
        ReadOnlySpan<char> chars = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, char>(ref _buffer), Count);
#endif
        return new(chars);
    }

    [Pure]
    public readonly bool Equals(scoped UnsafeBufferWriter<T> other) => Count == other.Count && Unsafe.AreSame(ref _buffer, ref other._buffer);

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => Count;

    public static bool operator ==(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => left.Equals(right);

    public static bool operator !=(UnsafeBufferWriter<T> left, UnsafeBufferWriter<T> right) => !(left == right);
}
