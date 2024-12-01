using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(PooledListBuilder), nameof(PooledListBuilder.Create))]
public sealed class PooledList<T> :
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

    T IIndexable<T>.this[Index index] => this[index];

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

    internal T[]? _buffer;

    private const int MinimumCapacity = 4;

    public PooledList() => _buffer = [];

    public PooledList(int capacity) => _buffer = capacity == 0 ? [] : ArrayPool<T>.Shared.Rent(capacity);

    public PooledList(ReadOnlySpan<T> items) : this(items.Length)
    {
        AssertBufferNotNull();
        SpanHelpers.Copy(items, _buffer);
        Count = items.Length;
    }

    public PooledList(ReadOnlySpan<T> items, int capacity) : this(int.Max(items.Length, capacity))
    {
        AssertBufferNotNull();
        SpanHelpers.Copy(items, _buffer);
        Count = items.Length;
    }

    public void Dispose()
    {
        T[]? buffer = _buffer;
        if (buffer is null)
        {
            return;
        }

        _buffer = null;
        ArrayPool<T>.Shared.Return(buffer);
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
        SpanHelpers.Copy(source, result);
        return result;
    }

    [Pure]
    public T[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public T[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public T[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<T> ToList() => AsSpan().ToList();

    [Pure]
    public List<T> ToList(int start) => AsSpan(start..).ToList();

    [Pure]
    public List<T> ToList(int start, int length) => AsSpan(start, length).ToList();

    [Pure]
    public List<T> ToList(Range range) => AsSpan(range).ToList();

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

    Memory<T> IMemoryProvider<T>.GetMemory() => AsMemory();

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.GetReadOnlyMemory() => AsMemory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private ref T GetDestination(int sizeHint)
    {
        Debug.Assert(sizeHint > 0);

        T[]? buffer = _buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<PooledList<T>>();
        }

        int count = Count;
        int freeBufferSpace = buffer.Length - count;
        if (freeBufferSpace >= sizeHint)
        {
            return ref ArrayMarshal.GetUnsafeElementAt(buffer, count);
        }

        return ref GrowAndGetDestination(sizeHint - freeBufferSpace);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private void GrowIfNeeded(int sizeHint)
    {
        Debug.Assert(sizeHint > 0);

        int freeSpace = Capacity - Count;
        if (freeSpace >= sizeHint)
        {
            return;
        }

        GrowAndGetDestination(sizeHint - freeSpace);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private ref T GrowAndGetDestination(int neededSize)
    {
        Debug.Assert(neededSize >= 0);

        int count = Count;
        T[] oldBuffer = GetBuffer();
        int newSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        T[] newBuffer = ArrayPool<T>.Shared.Rent(int.Max(newSize, MinimumCapacity));
        if (count != 0)
        {
            SpanHelpers.Copy(oldBuffer.AsSpan(..count), newBuffer);
        }

        ArrayPool<T>.Shared.Return(oldBuffer);
        _buffer = newBuffer;
        return ref ArrayMarshal.GetUnsafeElementAt(newBuffer, count);
    }

    public void Add(T item)
    {
        GetDestination(1) = item;
        Count++;
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
            AssertBufferNotNull();
            T[] buffer = _buffer;

            int count = Count;
            if (items.TryNonEnumeratedCopyTo(buffer, count, out _))
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

    public void AddRange(List<T> items)
        => AddRange(ref ListMarshal.GetReference(items), items.Count);

    public void AddRange(T[] items)
        => AddRange(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    public void AddRange(Span<T> items)
        => AddRange(ref MemoryMarshal.GetReference(items), items.Length);

    public void AddRange(params ReadOnlySpan<T> items)
        => AddRange(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddRange(ref T items, int length)
    {
        if (length == 0)
        {
            return;
        }

        SpanHelpers.Memmove(ref GetDestination(length), ref items, (uint)length);
        Count += length;
    }

    /// <summary>
    /// Trims unused buffer size.<br/>
    /// This method should ideally be called, when <see cref="Capacity"/> is much larger than <see cref="Count"/>.
    /// </summary>
    /// <example>
    /// After having removed a lot of items from the <see cref="PooledList{T}"/> <see cref="Capacity"/> will be much larger than <see cref="Count"/>.
    /// If there are 32 items remaining and the <see cref="Capacity"/> is 1024, the buffer of 1024 items will be returned to the <see cref="ArrayPool{T}"/>
    /// and a new buffer that has at least the size of the remaining items will be rented and the remaining 32 items are copied into it.
    /// </example>
    public void TrimBuffer()
    {
        int count = Count;
        T[] oldBuffer = GetBuffer();
        if (count == 0)
        {
            ArrayPool<T>.Shared.Return(oldBuffer);
            _buffer = [];
            return;
        }

        int trimmedBufferSize = BufferHelpers.GrowArray((uint)count, 0);
        if (trimmedBufferSize == oldBuffer.Length)
        {
            return;
        }

        T[] newBuffer = ArrayPool<T>.Shared.Rent(trimmedBufferSize);
        ref T source = ref MemoryMarshal.GetArrayDataReference(oldBuffer);
        ref T destination = ref MemoryMarshal.GetArrayDataReference(newBuffer);
        SpanHelpers.Memmove(ref destination, ref source, (uint)count);
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

        RemoveAt(index);
        return true;
    }

    [Pure]
    public int IndexOf(T item) => Array.IndexOf(GetBuffer(), item, 0, Count);

    public void Insert(int index, T item)
    {
        GrowIfNeeded(1);
        int count = Count;
        if (index > count)
        {
            GrowIfNeeded((index - count) + 1);
        }

        ref T buffer = ref GetBufferReference();
        ref T destination = ref Unsafe.Add(ref buffer, index);

        if (index != count)
        {
            SpanHelpers.Memmove(ref Unsafe.Add(ref destination, 1), ref destination, (uint)(count - index - 1));
        }

        destination = item;
        Count = count + 1;
    }

    public void RemoveAt(int index)
    {
        int count = Count;
        int lastIndex = count - 1;
        ref T buffer = ref GetBufferReference();

        if (index != lastIndex)
        {
            ref T source = ref Unsafe.Add(ref buffer, index + 1);
            ref T destination = ref Unsafe.Add(ref buffer, index);
            SpanHelpers.Memmove(ref destination, ref source, (uint)(count - index - 1));
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            ref T lastItem = ref Unsafe.Add(ref buffer, lastIndex);
            lastItem = default!; // remove reference from list
        }

        Count = lastIndex;
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

    [Conditional("DEBUG")]
    [MemberNotNull(nameof(_buffer))]
    private void AssertBufferNotNull() => Debug.Assert(_buffer is not null, $"{nameof(_buffer)} shouldn't be null. An exception should have been thrown before.");

    [Pure]
    public ArrayEnumerator<T> GetEnumerator() => new(GetBuffer(), 0, Count);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => Count == 0 ? EmptyEnumeratorCache<T>.Enumerator : GetEnumerator();

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
