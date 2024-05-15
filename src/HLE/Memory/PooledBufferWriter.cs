using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Strings;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Memory;

/// <summary>
/// Represents an output sink consisting of a buffer from an <see cref="ArrayPool{T}"/> into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
/// <param name="capacity">The starting capacity of the buffer.</param>
[method: MustDisposeResource]
[DebuggerDisplay("{ToString()}")]
public sealed class PooledBufferWriter<T>(int capacity) :
    IBufferWriter<T>,
    ICollection<T>,
    IDisposable,
    ICopyable<T>,
    IEquatable<PooledBufferWriter<T>>,
    IIndexable<T>,
    IReadOnlyCollection<T>,
    ISpanProvider<T>,
    ICollectionProvider<T>,
    IMemoryProvider<T>
{
    T IIndexable<T>.this[int index] => WrittenSpan[index];

    /// <summary>
    /// A <see cref="Span{T}"/> view over the written elements.
    /// </summary>
    public Span<T> WrittenSpan => GetBuffer().AsSpanUnsafe(..Count);

    /// <summary>
    /// A <see cref="Memory{T}"/> view over the written elements.
    /// </summary>
    public Memory<T> WrittenMemory => GetBuffer().AsMemory(..Count);

    /// <summary>
    /// The amount of written elements.
    /// </summary>
    public int Count { get; internal set; }

    public int Capacity => GetBuffer().Length;

    bool ICollection<T>.IsReadOnly => false;

    private T[]? _buffer = capacity == 0 ? [] : ArrayPool<T>.Shared.Rent(capacity);

    [MustDisposeResource]
    public PooledBufferWriter() : this(0)
    {
    }

    [MustDisposeResource]
    public PooledBufferWriter(ReadOnlySpan<T> data) : this(data.Length)
    {
        Debug.Assert(_buffer is not null);
        SpanHelpers<T>.Copy(data, _buffer);
        Count = data.Length;
    }

    public void Dispose()
    {
        T[]? buffer = _buffer;
        if (buffer is null)
        {
            return;
        }

        ArrayPool<T>.Shared.Return(buffer);
        _buffer = null;
    }

    /// <inheritdoc/>
    public void Advance(int count) => Count += count;

    /// <inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return GetBuffer().AsMemory(Count..);
    }

    /// <inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0) => MemoryMarshal.CreateSpan(ref GetReference(sizeHint), Capacity - Count);

    /// <summary>
    /// Returns a reference to the buffer to write the requested size (specified by sizeHint) to.
    /// </summary>
    /// <param name="sizeHint">The minimum size of free buffer space after the given reference. If 0, at least one space will be available.</param>
    /// <returns>A reference to the buffer that can be written to.</returns>
    public ref T GetReference(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), Count);
    }

    public void Write(T item)
    {
        GetReference() = item;
        Count++;
    }

    public void Write(IEnumerable<T> data)
    {
        if (data.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            Write(span);
            return;
        }

        switch (data)
        {
            case ICopyable<T> copyable:
                ref T destination = ref GetReference(copyable.Count);
                copyable.CopyTo(ref destination);
                Count += copyable.Count;
                return;
            case ICollection<T> collection:
                EnsureCapacity(Count + collection.Count);
                collection.CopyTo(GetBuffer(), Count);
                Count += collection.Count;
                return;
        }

        foreach (T item in data)
        {
            Write(item);
        }
    }

    public void Write(List<T> data) => Write(CollectionsMarshal.AsSpan(data));

    public void Write(T[] data) => Write(ref MemoryMarshal.GetArrayDataReference(data), data.Length);

    public void Write(Span<T> data) => Write(ref MemoryMarshal.GetReference(data), data.Length);

    public void Write(ReadOnlySpan<T> data) => Write(ref MemoryMarshal.GetReference(data), data.Length);

    public void Write(ref T data, int length)
    {
        ref T destination = ref GetReference(length);
        SpanHelpers<T>.Memmove(ref destination, ref data, (uint)length);
        Count += length;
    }

    public unsafe void Write(T* data, int length) => Write(ref Unsafe.AsRef<T>(data), length);

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
        Span<T> writtenSpan = WrittenSpan;
        if (writtenSpan.Length == 0)
        {
            return [];
        }

        T[] result = GC.AllocateUninitializedArray<T>(writtenSpan.Length);
        SpanHelpers<T>.Copy(writtenSpan, result);
        return result;
    }

    [Pure]
    public T[] ToArray(int start) => WrittenSpan.ToArray(start);

    [Pure]
    public T[] ToArray(int start, int length) => WrittenSpan.ToArray(start, length);

    [Pure]
    public T[] ToArray(Range range) => WrittenSpan.ToArray(range);

    [Pure]
    public List<T> ToList()
    {
        Span<T> writtenSpan = WrittenSpan;
        if (writtenSpan.Length == 0)
        {
            return [];
        }

        List<T> result = new(writtenSpan.Length);
        CopyWorker<T> copyWorker = new(writtenSpan);
        copyWorker.CopyTo(result);
        return result;
    }

    [Pure]
    public List<T> ToList(int start) => WrittenSpan.ToList(start);

    [Pure]
    public List<T> ToList(int start, int length) => WrittenSpan.ToList(start, length);

    [Pure]
    public List<T> ToList(Range range) => WrittenSpan.ToList(range);

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
        int count = Count;
        int trimmedBufferSize = BufferHelpers.GrowArray((uint)count, 0);
        if (trimmedBufferSize == Capacity)
        {
            return;
        }

        T[] oldBuffer = GetBuffer();
        try
        {
            if (trimmedBufferSize == 0)
            {
                _buffer = [];
                return;
            }

            T[] newBuffer = ArrayPool<T>.Shared.Rent(trimmedBufferSize);
            SpanHelpers<T>.Copy(oldBuffer, newBuffer);
            _buffer = newBuffer;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(oldBuffer);
        }
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
        Debug.Assert(neededSize >= 0);

        T[] oldBuffer = GetBuffer();
        int newBufferSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        T[] newBuffer = ArrayPool<T>.Shared.Rent(newBufferSize);
        if (Count != 0)
        {
            SpanHelpers<T>.Copy(oldBuffer, newBuffer);
        }

        ArrayPool<T>.Shared.Return(oldBuffer);
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

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => WrittenSpan;

    Memory<T> IMemoryProvider<T>.GetMemory() => WrittenMemory;

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.GetReadOnlyMemory() => WrittenMemory;

    void ICollection<T>.Add(T item) => Write(item);

    bool ICollection<T>.Contains(T item) => throw new NotSupportedException();

    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal T[] GetBuffer()
    {
        T[]? buffer = _buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<PooledBufferWriter<T>>();
        }

        return buffer;
    }

    [Pure]
    public override string ToString() => typeof(char) == typeof(T)
        ? new(Unsafe.As<char[]>(GetBuffer()).AsSpan(..Count))
        : ToStringHelpers.FormatCollection(this);

    public ArrayEnumerator<T> GetEnumerator() => new(GetBuffer(), 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => Count == 0 ? EmptyEnumeratorCache<T>.Enumerator : GetEnumerator();

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
