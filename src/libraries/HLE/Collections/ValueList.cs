using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Text;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(ValueListBuilder), nameof(ValueListBuilder.Create))]
public ref partial struct ValueList<T> :
    IList<T>,
    ICopyable<T>,
    IEquatable<ValueList<T>>,
    IDisposable,
    IIndexable<T>,
    IReadOnlyList<T>,
    ISpanProvider<T>,
    IReadOnlySpanProvider<T>,
    ICollectionProvider<T>
{
    public readonly ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);
            return ref Unsafe.Add(ref GetBufferReference(), index);
        }
    }

    readonly T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }

    readonly T IReadOnlyList<T>.this[int index] => this[index];

    readonly T IIndexable<T>.this[int index] => this[index];

    readonly T IIndexable<T>.this[Index index] => this[index];

    public readonly ref T this[Index index] => ref this[index.GetOffset(Count)];

    public readonly Span<T> this[Range range] => AsSpan(range);

    public int Count { get; internal set; }

    public int Capacity { get; private set; }

    readonly bool ICollection<T>.IsReadOnly => false;

    internal ref T _buffer;
    private Flags _flags;

    private const int MinimumCapacity = 4;

    public ValueList() => _buffer = ref Unsafe.NullRef<T>();

    public ValueList(int capacity)
    {
        T[] buffer = ArrayPool<T>.Shared.Rent(int.Max(capacity, MinimumCapacity));
        _buffer = ref MemoryMarshal.GetArrayDataReference(buffer);
        Capacity = buffer.Length;
        _flags = Flags.IsRentedArray;
    }

    public ValueList(Span<T> buffer)
    {
        _buffer = ref MemoryMarshal.GetReference(buffer);
        Capacity = buffer.Length;
    }

    public void Dispose()
    {
        Flags flags = _flags;
        if ((flags & Flags.IsDisposed) != 0)
        {
            return;
        }

        _flags = Flags.IsDisposed;

        ref T buffer = ref _buffer;
        _buffer = ref Unsafe.NullRef<T>();

        if ((flags & Flags.IsRentedArray) == 0)
        {
            return;
        }

        T[] array = SpanMarshal.AsArray(ref buffer);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SpanHelpers.Clear(array, Count);
        }

        ArrayPool<T>.Shared.Return(array);
    }

    [Pure]
    public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref GetBufferReference(), Count);

    [Pure]
    public readonly Span<T> AsSpan(int start) => Slicer.Slice(ref GetBufferReference(), Count, start);

    [Pure]
    public readonly Span<T> AsSpan(int start, int length) => Slicer.Slice(ref GetBufferReference(), Count, start, length);

    [Pure]
    public readonly Span<T> AsSpan(Range range) => Slicer.Slice(ref GetBufferReference(), Count, range);

    readonly ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan() => AsSpan();

    readonly ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(int start) => AsSpan(start..);

    readonly ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(int start, int length) => AsSpan(start, length);

    readonly ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(Range range) => AsSpan(range);

    [Pure]
    public readonly T[] ToArray()
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
    public readonly T[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public readonly T[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public readonly T[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public readonly List<T> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public readonly List<T> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public readonly List<T> ToList(Range range) => AsSpan().ToList(range);

    [Pure]
    public readonly List<T> ToList() => AsSpan().ToList();

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private ref T GetDestination(int sizeHint)
    {
        int count = Count;
        int freeSpace = Capacity - count;
        if (freeSpace >= sizeHint)
        {
            return ref Unsafe.Add(ref GetBufferReference(), count);
        }

        return ref GrowAndGetDestination(sizeHint - freeSpace);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private ref T GrowAndGetDestination(int neededSize)
    {
        Debug.Assert(neededSize >= 0);

        Span<T> oldBuffer = GetBuffer();
        int newSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        Span<T> newBuffer = ArrayPool<T>.Shared.Rent(int.Max(newSize, MinimumCapacity));
        int count = Count;
        if (count != 0)
        {
            SpanHelpers.Copy(oldBuffer[..count], newBuffer);
        }

        Flags flags = _flags;

        ref T buffer = ref MemoryMarshal.GetReference(newBuffer);
        _buffer = ref buffer;
        Capacity = newBuffer.Length;
        _flags |= Flags.IsRentedArray;

        if ((flags & Flags.IsRentedArray) == 0)
        {
            return ref Unsafe.Add(ref buffer, count);
        }

        T[] array = SpanMarshal.AsArray(oldBuffer);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SpanHelpers.Clear(array, count);
        }

        ArrayPool<T>.Shared.Return(array);

        return ref Unsafe.Add(ref buffer, count);
    }

    public void Add(T item)
    {
        GetDestination(1) = item;
        Count++;
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetNonEnumeratedCount(out int elementCount))
        {
            if (elementCount == 0)
            {
                return;
            }

            int count = Count;
            ref T destination = ref GetDestination(elementCount);
            if (items.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
            {
                SpanHelpers.Copy(span, ref destination);
                Count = count + elementCount;
                return;
            }

            if ((_flags & Flags.IsRentedArray) != 0 && items is ICollection<T> collection)
            {
                T[] buffer = SpanMarshal.AsArray(GetBuffer());
                Debug.Assert(buffer.GetType() == typeof(T[]));
                Debug.Assert(buffer.Length >= Count + collection.Count);
                collection.CopyTo(buffer, Count);
                Count = count + collection.Count;
                return;
            }

            foreach (T item in items)
            {
                destination = item;
                destination = ref Unsafe.Add(ref destination, 1);
            }

            Count = count + elementCount;
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

    public void AddRange(scoped Span<T> items)
        => AddRange(ref MemoryMarshal.GetReference(items), items.Length);

    public void AddRange(params scoped ReadOnlySpan<T> items)
        => AddRange(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddRange(ref T items, int length)
    {
        SpanHelpers.Memmove(ref GetDestination(length), ref items, length);
        Count += length;
    }

    /// <summary>
    /// Trims unused buffer size.<br/>
    /// This method should ideally be called, when <see cref="Capacity"/> of the <see cref="ValueList{T}"/> is much larger than <see cref="Count"/>.
    /// </summary>
    /// <example>
    /// After having removed a lot of items from the <see cref="ValueList{T}"/> <see cref="Capacity"/> will be much larger than <see cref="Count"/>.
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

        Span<T> oldBuffer = GetBuffer();
        if (trimmedBufferSize == 0)
        {
            Debug.Assert(count == 0);
            if ((_flags & Flags.IsRentedArray) != 0)
            {
                _flags &= ~Flags.IsRentedArray;
                T[] array = SpanMarshal.AsArray(oldBuffer);
                ArrayPool<T>.Shared.Return(array);
            }

            _buffer = ref Unsafe.NullRef<T>();
            Capacity = 0;
            return;
        }

        T[] newBuffer = ArrayPool<T>.Shared.Rent(trimmedBufferSize);
        ref T source = ref MemoryMarshal.GetReference(oldBuffer);
        ref T destination = ref MemoryMarshal.GetArrayDataReference(newBuffer);
        SpanHelpers.Memmove(ref destination, ref source, count);

        if ((_flags & Flags.IsRentedArray) != 0)
        {
            _flags &= ~Flags.IsRentedArray;

            T[] array = SpanMarshal.AsArray(oldBuffer);

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                SpanHelpers.Clear(array, count);
            }

            ArrayPool<T>.Shared.Return(array);
        }

        _buffer = ref MemoryMarshal.GetArrayDataReference(newBuffer);
        Capacity = newBuffer.Length;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SpanHelpers.Clear(ref GetBufferReference(), Count);
        }

        Count = 0;
    }

    public void EnsureCapacity(int capacity) => GetDestination(capacity - Capacity);

    readonly int IList<T>.IndexOf(T item) => throw new NotSupportedException();

    readonly bool ICollection<T>.Contains(T item) => throw new NotSupportedException();

    public void Insert(int index, T item)
    {
        GetDestination(1);
        Count++;
        Span<T> buffer = AsSpan();
        buffer[index..^1].CopyTo(buffer[(index + 1)..]);
        buffer[index] = item;
    }

    readonly bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    public void RemoveAt(int index)
    {
        Span<T> buffer = AsSpan();
        buffer[(index + 1)..].CopyTo(buffer[index..]);
        Count--;
    }

    public readonly void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(scoped Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(scoped ref T destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(ref destination);
    }

    public readonly unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    private readonly ref T GetBufferReference()
    {
        if ((_flags & Flags.IsDisposed) != 0)
        {
            ThrowHelper.ThrowObjectDisposedException<ValueList<T>>();
        }

        return ref _buffer;
    }

    private readonly Span<T> GetBuffer() => MemoryMarshal.CreateSpan(ref GetBufferReference(), Capacity);

    public override readonly string ToString()
    {
        int count = Count;
        if (typeof(T) != typeof(char))
        {
            return ToStringHelpers.FormatCollection<ValueList<T>>(count);
        }

        ref char reference = ref Unsafe.As<T, char>(ref GetBufferReference());
        return new(MemoryMarshal.CreateReadOnlySpan(ref reference, count));
    }

    [Pure]
    public readonly MemoryEnumerator<T> GetEnumerator() => new(AsSpan());

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException();

    readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    [Pure]
    public readonly bool Equals(scoped ValueList<T> other) => Count == other.Count && GetBuffer() == other.GetBuffer();

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(Count, Capacity, _flags);

    public static bool operator ==(ValueList<T> left, ValueList<T> right) => left.Equals(right);

    public static bool operator !=(ValueList<T> left, ValueList<T> right) => !(left == right);
}
