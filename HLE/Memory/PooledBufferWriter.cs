using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Strings;

namespace HLE.Memory;

/// <summary>
/// Represents an output sink consisting of buffers from an <see cref="ArrayPool{T}"/> into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
[DebuggerDisplay("{ToString()}")]
public sealed class PooledBufferWriter<T>(int capacity)
    : IBufferWriter<T>, ICollection<T>, IDisposable, ICopyable<T>, ICountable, IEquatable<PooledBufferWriter<T>>, IIndexAccessible<T>,
        IReadOnlyCollection<T>, ISpanProvider<T>, ICollectionProvider<T>
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
    public int Count { get; private set; }

    public int Capacity => _buffer.Length;

    bool ICollection<T>.IsReadOnly => false;

    [SuppressMessage("ReSharper", "NotDisposedResource", Justification = "disposed in Dispose()")]
    internal RentedArray<T> _buffer = capacity == 0 ? [] : ArrayPool<T>.Shared.RentAsRentedArray(capacity);

    public PooledBufferWriter() : this(0)
    {
    }

    public PooledBufferWriter(ReadOnlySpan<T> data) : this(data.Length)
    {
        CopyWorker<T>.Copy(data, _buffer.AsSpan());
        Count = data.Length;
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
        GrowIfNeeded(sizeHint);
        return ref Unsafe.Add(ref _buffer.Reference, Count);
    }

    public void Write(T item)
    {
        GetReference() = item;
        Count++;
    }

    public void Write(List<T> data) => Write(CollectionsMarshal.AsSpan(data));

    public void Write(T[] data) => Write(data.AsSpan());

    public void Write(Span<T> data) => Write((ReadOnlySpan<T>)data);

    public void Write(ReadOnlySpan<T> data)
    {
        ref T buffer = ref GetReference(data.Length);
        CopyWorker<T>.Copy(data, ref buffer);
        Count += data.Length;
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
        if (Count == 0)
        {
            return [];
        }

        T[] result = GC.AllocateUninitializedArray<T>(Count);
        CopyWorker<T>.Copy(WrittenSpan, result);
        return result;
    }

    [Pure]
    public List<T> ToList()
    {
        if (Count == 0)
        {
            return [];
        }

        List<T> result = new(Count);
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Trims unused buffer size.<br/>
    /// This method should ideally be called, when <see cref="Capacity"/> is much larger than <see cref="Count"/>.
    /// </summary>
    /// <example>
    /// After having removed a lot of items from the <see cref="PooledBufferWriter{T}"/> <see cref="Capacity"/> will be much larger than <see cref="Count"/>.
    /// If there are 32 items remaining and the <see cref="Capacity"/> is 1024, the buffer of 1024 items will be returned to the <see cref="ArrayPool{T}"/>
    /// and a new buffer that has at least the size of the remaining items will be rented and the remaining 32 items are copied into it.
    /// </example>
    public void TrimBuffer()
    {
        int trimmedBufferSize = BufferHelpers.GrowArray(Count, 0);
        if (trimmedBufferSize == Capacity)
        {
            return;
        }

        if (trimmedBufferSize == 0)
        {
            _buffer.Dispose();
            _buffer = [];
            return;
        }

        using RentedArray<T> oldBuffer = _buffer;
        RentedArray<T> newBuffer = ArrayPool<T>.Shared.RentAsRentedArray(trimmedBufferSize);
        CopyWorker<T>.Copy(ref oldBuffer.Reference, ref newBuffer.Reference, (uint)Count);
        _buffer = newBuffer;
    }

    /// <summary>
    /// Grows the buffer if <paramref name="sizeHint"/> amount of elements won't fit into the buffer.
    /// </summary>
    /// <param name="sizeHint">The amount of elements waiting to be written.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
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

        Grow(sizeHint - freeSpace);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private void Grow(int neededSize)
    {
        using RentedArray<T> oldBuffer = _buffer;
        int newBufferSize = BufferHelpers.GrowArray(oldBuffer.Length, neededSize);
        RentedArray<T> newBuffer = ArrayPool<T>.Shared.RentAsRentedArray(newBufferSize);
        if (Count != 0)
        {
            CopyWorker<T>.Copy(ref oldBuffer.Reference, ref newBuffer.Reference, (uint)Count);
        }

        _buffer = newBuffer;
    }

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

    void ICollection<T>.Add(T item) => Write(item);

    bool ICollection<T>.Contains(T item) => throw new NotSupportedException();

    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    [Pure]
    public override string ToString()
        => typeof(char) == typeof(T)
            ? Unsafe.As<RentedArray<T>, RentedArray<char>>(ref _buffer).ToString()
            : ToStringHelpers.FormatCollection(this);

    public ArrayEnumerator<T> GetEnumerator() => new(_buffer.Array, 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledBufferWriter<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledBufferWriter<T>? left, PooledBufferWriter<T>? right) => Equals(left, right);

    public static bool operator !=(PooledBufferWriter<T>? left, PooledBufferWriter<T>? right) => !(left == right);
}
