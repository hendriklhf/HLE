using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

/// <summary>
/// Represents an output sink consisting of a buffer from an <see cref="ArrayPool{T}"/> into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
[DebuggerDisplay("{ToString()}")]
public sealed class PooledBufferWriter<T> :
    IBufferWriter<T>,
    ICollection<T>,
    IDisposable,
    ICopyable<T>,
    IEquatable<PooledBufferWriter<T>>,
    IIndexable<T>,
    IReadOnlyCollection<T>,
    ISpanProvider<T>,
    IReadOnlySpanProvider<T>,
    IMemoryProvider<T>,
    IReadOnlyMemoryProvider<T>,
    ICollectionProvider<T>
{
    T IIndexable<T>.this[int index] => WrittenSpan[index];

    T IIndexable<T>.this[Index index] => WrittenSpan[index];

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

    private T[]? _buffer;

    private const int MinimumCapacity = 4;

    public PooledBufferWriter() => _buffer = [];

    public PooledBufferWriter(int capacity) => _buffer = ArrayPool<T>.Shared.Rent(int.Max(capacity, MinimumCapacity));

    public PooledBufferWriter(ReadOnlySpan<T> data) : this(data.Length)
    {
        Debug.Assert(_buffer is not null);
        SpanHelpers.Copy(data, _buffer);
        Count = data.Length;
    }

    public void Dispose()
    {
        T[]? buffer = Interlocked.Exchange(ref _buffer, null);
        if (buffer is null)
        {
            return;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SpanHelpers.Clear(buffer, Count);
        }

        ArrayPool<T>.Shared.Return(buffer);
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
        return ref ArrayMarshal.GetUnsafeElementAt(GetBuffer(), Count);
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
        SpanHelpers.Memmove(ref destination, ref data, length);
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
    public Span<T> AsSpan() => GetBuffer().AsSpanUnsafe(..Count);

    [Pure]
    public Span<T> AsSpan(int start) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), Count, start);

    [Pure]
    public Span<T> AsSpan(int start, int length) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), Count, start, length);

    [Pure]
    public Span<T> AsSpan(Range range) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), Count, range);

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan() => AsSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(int start) => AsSpan(start..);

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(int start, int length) => AsSpan(start, length);

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(Range range) => AsSpan(range);

    [Pure]
    public Memory<T> AsMemory() => GetBuffer().AsMemory(0, Count);

    [Pure]
    public Memory<T> AsMemory(int start) => AsMemory()[start..];

    [Pure]
    public Memory<T> AsMemory(int start, int length) => AsMemory().Slice(start, length);

    [Pure]
    public Memory<T> AsMemory(Range range) => AsMemory()[range];

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory() => AsMemory();

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory(int start) => AsMemory(start..);

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory(int start, int length) => AsMemory(start, length);

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory(Range range) => AsMemory(range);

    [Pure]
    public T[] ToArray()
    {
        Span<T> writtenSpan = WrittenSpan;
        if (writtenSpan.Length == 0)
        {
            return [];
        }

        T[] result = GC.AllocateUninitializedArray<T>(writtenSpan.Length);
        SpanHelpers.Copy(writtenSpan, result);
        return result;
    }

    [Pure]
    public T[] ToArray(int start) => WrittenSpan.ToArray(start);

    [Pure]
    public T[] ToArray(int start, int length) => WrittenSpan.ToArray(start, length);

    [Pure]
    public T[] ToArray(Range range) => WrittenSpan.ToArray(range);

    [Pure]
    public List<T> ToList() => WrittenSpan.ToList();

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
        int trimmedBufferSize = BufferHelpers.GrowArray((uint)Count, 0);
        if (trimmedBufferSize == Capacity)
        {
            return;
        }

        T[] oldBuffer = GetBuffer();
        if (trimmedBufferSize == 0)
        {
            _buffer = [];

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                SpanHelpers.Clear(oldBuffer, Count);
            }

            ArrayPool<T>.Shared.Return(oldBuffer);
            return;
        }

        T[] newBuffer = ArrayPool<T>.Shared.Rent(trimmedBufferSize);
        SpanHelpers.Copy(oldBuffer, newBuffer);
        _buffer = newBuffer;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SpanHelpers.Clear(oldBuffer, Count);
        }

        ArrayPool<T>.Shared.Return(oldBuffer);
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
        newBufferSize = int.Max(newBufferSize, MinimumCapacity);

        T[] newBuffer = ArrayPool<T>.Shared.Rent(newBufferSize);
        if (Count != 0)
        {
            SpanHelpers.Copy(oldBuffer, newBuffer);

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                SpanHelpers.Clear(oldBuffer, Count);
            }
        }

        ArrayPool<T>.Shared.Return(oldBuffer);
        _buffer = newBuffer;
    }

    public void CopyTo(List<T> destination, int offset = 0)
        => SpanHelpers.CopyChecked(WrittenSpan, destination, offset);

    public void CopyTo(T[] destination, int offset = 0)
        => SpanHelpers.CopyChecked(WrittenSpan, destination.AsSpan(offset..));

    public void CopyTo(Memory<T> destination) => SpanHelpers.CopyChecked(WrittenSpan, destination.Span);

    public void CopyTo(Span<T> destination) => SpanHelpers.CopyChecked(WrittenSpan, destination);

    public void CopyTo(ref T destination) => SpanHelpers.Copy(WrittenSpan, ref destination);

    public unsafe void CopyTo(T* destination) => SpanHelpers.Copy(WrittenSpan, destination);

    void ICollection<T>.Add(T item) => Write(item);

    bool ICollection<T>.Contains(T item) => Array.IndexOf(GetBuffer(), item, 0, Count) >= 0;

    bool ICollection<T>.Remove(T item)
    {
        T[] buffer = GetBuffer();
        int index = Array.IndexOf(buffer, item, 0, Count);
        if (index < 0)
        {
            return false;
        }

        ref T src = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(buffer), index + 1);
        ref T dst = ref Unsafe.Add(ref src, -1);
        SpanHelpers.Memmove(ref dst, ref src, Count - index - 1);
        return true;
    }

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
        ? new(Unsafe.As<char[]>(GetBuffer()).AsSpanUnsafe(0, Count))
        : ToStringHelpers.FormatCollection(this);

    public ArrayEnumerator<T> GetEnumerator() => new(GetBuffer(), 0, Count);

    // ReSharper disable once NotDisposedResourceIsReturned
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
