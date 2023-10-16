using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Memory;

/// <summary>
/// Represents an output sink consisting of buffers from an <see cref="ArrayPool{T}"/> into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
[DebuggerDisplay("{ToString()}")]
public sealed class PooledBufferWriter<T>(int capacity)
    : IBufferWriter<T>, ICollection<T>, IDisposable, ICopyable<T>, ICountable, IEquatable<PooledBufferWriter<T>>, IIndexAccessible<T>, IReadOnlyCollection<T>, ISpanProvider<T>
{
    T IIndexAccessible<T>.this[int index] => WrittenSpan[index];

    /// <summary>
    /// A <see cref="Span{T}"/> view over the written elements.
    /// </summary>
    public Span<T> WrittenSpan => _buffer.AsSpan(..Count);

    /// <summary>
    /// A <see cref="Memory{T}"/> view over the written elements.
    /// </summary>
    public Memory<T> WrittenMemory => _buffer.AsMemory(..Count);

    /// <summary>
    /// The amount of written elements.
    /// </summary>
    public int Count { get; internal set; }

    public int Capacity => _buffer.Length;

    int ICollection<T>.Count => Count;

    bool ICollection<T>.IsReadOnly => false;

    internal RentedArray<T> _buffer = ArrayPool<T>.Shared.CreateRentedArray(capacity);

    private const int _defaultCapacity = ArrayPool<T>.MinimumArrayLength;
    private const int _maximumCapacity = 1 << 30;

    public PooledBufferWriter() : this(_defaultCapacity)
    {
    }

    public void Dispose() => _buffer.Dispose();

    /// <inheritdoc/>
    public void Advance(int count) => Count += count;

    /// <inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _buffer.AsMemory(Count..);
    }

    /// <inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _buffer.AsSpan(Count..);
    }

    /// <summary>
    /// Returns a reference to the buffer to write the requested size (specified by sizeHint) to.
    /// </summary>
    /// <param name="sizeHint">The minimum size of free buffer space after the given reference. If 0, at least one space will be available.</param>
    /// <returns>A reference to the buffer that can be written to.</returns>
    public ref T GetReference(int sizeHint = 0)
    {
        Span<T> buffer = GetSpan(sizeHint);
        return ref MemoryMarshal.GetReference(buffer);
    }

    public void Write(List<T> data) => Write(CollectionsMarshal.AsSpan(data));

    public void Write(T[] data) => Write((ReadOnlySpan<T>)data);

    public void Write(Span<T> data) => Write((ReadOnlySpan<T>)data);

    public void Write(ReadOnlySpan<T> data)
    {
        Span<T> buffer = GetSpan(data.Length);
        data.CopyToUnsafe(buffer);
        Advance(data.Length);
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            WrittenSpan.Clear();
        }

        Count = 0;
    }

    public void EnsureCapacity(int capacity) => GrowIfNeeded(capacity - Capacity);

    [Pure]
    public T[] ToArray()
    {
        T[] result = GC.AllocateUninitializedArray<T>(Count);
        CopyWorker<T>.Memmove(ref MemoryMarshal.GetArrayDataReference(result), ref _buffer.Reference, (nuint)Count);
        return result;
    }

    [Pure]
    public List<T> ToList()
    {
        List<T> result = new(Count);
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Grows the buffer if <paramref name="sizeHint"/> amount of elements won't fit into the buffer.
    /// </summary>
    /// <param name="sizeHint">The amount of elements waiting to be written.</param>
    private void GrowIfNeeded(int sizeHint)
    {
        if (sizeHint < 1)
        {
            sizeHint = 1;
        }

        int freeSpace = Capacity - Count;
        if (freeSpace >= sizeHint)
        {
            return;
        }

        if (Capacity == _maximumCapacity)
        {
            ThrowMaximumBufferCapacityReached();
        }

        int neededSpace = sizeHint - freeSpace;
        int newBufferSize = (int)BitOperations.RoundUpToPowerOf2((uint)(_buffer.Length + neededSpace));
        if (newBufferSize < Capacity)
        {
            ThrowMaximumBufferCapacityReached();
        }

        using RentedArray<T> oldBuffer = _buffer;
        _buffer = ArrayPool<T>.Shared.CreateRentedArray(newBufferSize);
        CopyWorker<T> copyWorker = new(ref oldBuffer.Reference, Count);
        copyWorker.CopyTo(ref _buffer.Reference);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaximumBufferCapacityReached()
        => throw new InvalidOperationException("The maximum buffer capacity has been reached.");

    public void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    Span<T> ISpanProvider<T>.GetSpan() => WrittenSpan;

    void ICollection<T>.Add(T item)
    {
        GetReference() = item;
        Advance(1);
    }

    bool ICollection<T>.Contains(T item) => _buffer.Contains(item);

    bool ICollection<T>.Remove(T item)
    {
        int index = Array.IndexOf(_buffer.Array, item);
        if (index < 0)
        {
            return false;
        }

        _buffer[(index + 1)..].CopyTo(_buffer[index..]);
        Advance(-1);
        return true;
    }

    [Pure]
    public bool Equals(PooledBufferWriter<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [Pure]
    public override string ToString()
    {
        if (typeof(char) == typeof(T))
        {
            ref char charsReference = ref Unsafe.As<T, char>(ref _buffer.Reference);
            ReadOnlySpan<char> chars = MemoryMarshal.CreateReadOnlySpan(ref charsReference, Count);
            return new(chars);
        }

        Type thisType = typeof(PooledBufferWriter<T>);
        Type genericType = typeof(T);
        return $"{thisType.Namespace}.{nameof(PooledBufferWriter<T>)}<{genericType.Namespace}.{genericType.Name}>[{Count}]";
    }

    public ArrayEnumerator<T> GetEnumerator() => new(_buffer.Array, 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
