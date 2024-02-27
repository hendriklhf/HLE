using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[method: MustDisposeResource]
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(PooledListBuilder), nameof(PooledListBuilder.Create))]
public sealed class PooledList<T>(int capacity) :
    IList<T>,
    ICopyable<T>,
    IEquatable<PooledList<T>>,
    IDisposable,
    IIndexable<T>,
    IReadOnlyList<T>,
    ISpanProvider<T>,
    ICollectionProvider<T>,
    IMemoryProvider<T>
{
    public ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);
            return ref Unsafe.Add(ref GetBufferReference(), index);
        }
    }

    T IIndexable<T>.this[int index] => this[index];

    T IReadOnlyList<T>.this[int index] => this[index];

    T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }

    public ref T this[Index index] => ref this[index.GetOffset(Count)];

    public Span<T> this[Range range] => AsSpan(range);

    public int Count { get; internal set; }

    public int Capacity => GetBuffer().Length;

    bool ICollection<T>.IsReadOnly => false;

    internal T[]? _buffer = capacity == 0 ? [] : ArrayPool<T>.Shared.Rent(capacity);

    [MustDisposeResource]
    public PooledList() : this(0)
    {
    }

    [MustDisposeResource]
    public PooledList(ReadOnlySpan<T> items) : this(items.Length)
    {
        SpanHelpers<T>.Copy(items, _buffer.AsSpan());
        Count = items.Length;
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

    [Pure]
    public Span<T> AsSpan() => GetBuffer().AsSpanUnsafe(..Count);

    [Pure]
    public Span<T> AsSpan(int start) => new Slicer<T>(ref GetBufferReference(), Count).SliceSpan(start);

    [Pure]
    public Span<T> AsSpan(int start, int length) => new Slicer<T>(ref GetBufferReference(), Count).SliceSpan(start, length);

    [Pure]
    public Span<T> AsSpan(Range range) => new Slicer<T>(ref GetBufferReference(), Count).SliceSpan(range);

    [Pure]
    public Memory<T> AsMemory() => GetBuffer().AsMemory(..Count);

    [Pure]
    public T[] ToArray()
    {
        Span<T> source = AsSpan();
        if (source.Length == 0)
        {
            return [];
        }

        T[] result = GC.AllocateUninitializedArray<T>(source.Length);
        SpanHelpers<T>.Copy(source, result);
        return result;
    }

    [Pure]
    public T[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public T[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public T[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<T> ToList()
    {
        Span<T> source = AsSpan();
        if (source.Length == 0)
        {
            return [];
        }

        List<T> result = new(source.Length);
        CopyWorker<T> copyWorker = new(source);
        copyWorker.CopyTo(result);
        return result;
    }

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

    Memory<T> IMemoryProvider<T>.GetMemory() => AsMemory();

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.GetReadOnlyMemory() => AsMemory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private void GrowIfNeeded(int sizeHint)
    {
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

        int count = Count;
        T[] oldBuffer = GetBuffer();
        int newSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        T[] newBuffer = ArrayPool<T>.Shared.Rent(newSize);
        if (count != 0)
        {
            SpanHelpers<T>.Copy(oldBuffer.AsSpan(..count), newBuffer);
        }

        ArrayPool<T>.Shared.Return(oldBuffer);
        _buffer = newBuffer;
    }

    public void Add(T item)
    {
        GrowIfNeeded(1);
        Unsafe.Add(ref GetBufferReference(), Count++) = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetNonEnumeratedCount(out int itemsCount))
        {
            if (itemsCount == 0)
            {
                return;
            }

            GrowIfNeeded(itemsCount);
            T[] buffer = GetBuffer();
            int count = Count;
            if (items.TryNonEnumeratedCopyTo(buffer, count))
            {
                Count = count + itemsCount;
                return;
            }

            ref T destination = ref MemoryMarshal.GetArrayDataReference(buffer);
            foreach (T item in items)
            {
                Unsafe.Add(ref destination, count++) = item;
            }

            Count = count;
            return;
        }

        foreach (T item in items)
        {
            Add(item);
        }
    }

    public void AddRange(List<T> items) => AddRange((ReadOnlySpan<T>)CollectionsMarshal.AsSpan(items));

    public void AddRange(params T[] items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(Span<T> items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(ReadOnlySpan<T> items)
    {
        if (items.Length == 0)
        {
            return;
        }

        GrowIfNeeded(items.Length);
        int count = Count;
        ref T destination = ref Unsafe.Add(ref GetBufferReference(), count);
        SpanHelpers<T>.Copy(items, ref destination);
        Count = count + items.Length;
    }

    /// <summary>
    /// Trims unused buffer size.<br/>
    /// This method should ideally be called, when <see cref="Capacity"/> of the <see cref="PooledList{T}"/> is much larger than <see cref="Count"/>.
    /// </summary>
    /// <example>
    /// After having removed a lot of items from the <see cref="PooledList{T}"/> <see cref="Capacity"/> will be much larger than <see cref="Count"/>.
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
        if (trimmedBufferSize == 0)
        {
            ArrayPool<T>.Shared.Return(oldBuffer);
            _buffer = [];
            return;
        }

        T[] newBuffer = ArrayPool<T>.Shared.Rent(trimmedBufferSize);
        ref T source = ref MemoryMarshal.GetArrayDataReference(oldBuffer);
        ref T destination = ref MemoryMarshal.GetArrayDataReference(newBuffer);
        SpanHelpers<T>.Memmove(ref destination, ref source, (uint)count);
        _buffer = newBuffer;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            AsSpan().Clear();
        }

        Count = 0;
    }

    public void EnsureCapacity(int capacity) => GrowIfNeeded(capacity - Capacity);

    [Pure]
    public bool Contains(T item) => IndexOf(item) >= 0;

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        T[] buffer = GetBuffer();
        buffer.AsSpan((index + 1)..).CopyTo(buffer.AsSpan(index..));
        Count--;
        return true;
    }

    [Pure]
    public int IndexOf(T item) => Array.IndexOf(GetBuffer(), item, 0, Count);

    public void Insert(int index, T item)
    {
        GrowIfNeeded(1);
        Count++;
        Span<T> buffer = AsSpan();
        buffer[index..^1].CopyTo(buffer[(index + 1)..]);
        buffer[index] = item;
    }

    public void RemoveAt(int index)
    {
        Span<T> buffer = AsSpan();
        buffer[(index + 1)..].CopyTo(buffer[index..]);
        Count--;
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal T[] GetBuffer()
    {
        T[]? buffer = _buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<PooledList<T>>();
        }

        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetBufferReference() => ref MemoryMarshal.GetArrayDataReference(GetBuffer());

    [Pure]
    public ArrayEnumerator<T> GetEnumerator() => new(GetBuffer(), 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledList<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledList<T>? left, PooledList<T>? right) => Equals(left, right);

    public static bool operator !=(PooledList<T>? left, PooledList<T>? right) => !(left == right);
}
