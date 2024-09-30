using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Text;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(ValueListBuilder), nameof(ValueListBuilder.Create))]
public ref struct ValueList<T> :
    IList<T>,
    ICopyable<T>,
    IEquatable<ValueList<T>>,
    IDisposable,
    IIndexable<T>,
    IReadOnlyList<T>,
    ISpanProvider<T>,
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

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _countAndIsStackalloced.Integer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set
        {
            Debug.Assert(value >= 0);
            _countAndIsStackalloced.SetIntegerUnsafe(value);
        }
    }

    private bool IsStackalloced
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _countAndIsStackalloced.Bool;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _countAndIsStackalloced.Bool = value;
    }

    private int BufferLength
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _bufferLengthAndIsDisposed.Integer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            Debug.Assert(value >= 0);
            _bufferLengthAndIsDisposed.SetIntegerUnsafe(value);
        }
    }

    private bool IsDisposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _bufferLengthAndIsDisposed.Bool;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _bufferLengthAndIsDisposed.Bool = value;
    }

    public readonly int Capacity => BufferLength;

    readonly bool ICollection<T>.IsReadOnly => false;

    internal ref T _buffer;
    private IntBoolUnion<int> _bufferLengthAndIsDisposed;
    private IntBoolUnion<int> _countAndIsStackalloced;

    [MustDisposeResource]
    public ValueList()
    {
        _buffer = ref Unsafe.NullRef<T>();
        IsStackalloced = true;
    }

    [MustDisposeResource]
    public ValueList(int capacity)
    {
        T[] buffer = ArrayPool<T>.Shared.Rent(capacity);
        _buffer = ref MemoryMarshal.GetArrayDataReference(buffer);
        BufferLength = buffer.Length;
    }

    [MustDisposeResource]
    public ValueList(Span<T> buffer)
    {
        _buffer = ref MemoryMarshal.GetReference(buffer);
        BufferLength = buffer.Length;
        IsStackalloced = true;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        if (IsStackalloced)
        {
            _buffer = ref Unsafe.NullRef<T>();
            BufferLength = 0;
            IsDisposed = true;
            return;
        }

        T[] array = SpanMarshal.AsArray(ref _buffer);
        ArrayPool<T>.Shared.Return(array);

        _buffer = ref Unsafe.NullRef<T>();
        BufferLength = 0;
        IsDisposed = true;
    }

    [Pure]
    public readonly Span<T> AsSpan() => GetBuffer().SliceUnsafe(..Count);

    [Pure]
    public readonly Span<T> AsSpan(int start) => new Slicer<T>(ref GetBufferReference(), Count).SliceSpan(start);

    [Pure]
    public readonly Span<T> AsSpan(int start, int length) => new Slicer<T>(ref GetBufferReference(), Count).SliceSpan(start, length);

    [Pure]
    public readonly Span<T> AsSpan(Range range) => new Slicer<T>(ref GetBufferReference(), Count).SliceSpan(range);

    readonly Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    readonly ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

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
        Span<T> newBuffer = ArrayPool<T>.Shared.Rent(newSize);
        int count = Count;
        if (count != 0)
        {
            SpanHelpers.Copy(oldBuffer[..count], newBuffer);
        }

        ref T buffer = ref MemoryMarshal.GetReference(newBuffer);
        _buffer = ref buffer;
        BufferLength = newBuffer.Length;

        if (IsStackalloced)
        {
            IsStackalloced = false;
            return ref Unsafe.Add(ref buffer, count);
        }

        T[] array = SpanMarshal.AsArray(oldBuffer);
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
        if (items.TryGetNonEnumeratedCount(out int itemsCount))
        {
            if (itemsCount == 0)
            {
                return;
            }

            int count = Count;
            ref T destination = ref GetDestination(itemsCount);
            if (items.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
            {
                SpanHelpers.Copy(span, ref destination);
                Count = count + itemsCount;
                return;
            }

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

    public void AddRange(scoped Span<T> items)
        => AddRange(ref MemoryMarshal.GetReference(items), items.Length);

    public void AddRange(params scoped ReadOnlySpan<T> items)
        => AddRange(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddRange(ref T items, int length)
    {
        if (length == 0)
        {
            return;
        }

        int count = Count;
        SpanHelpers.Memmove(ref GetDestination(length), ref items, (uint)length);
        Count = count + length;
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
            if (!IsStackalloced)
            {
                T[] array = SpanMarshal.AsArray(oldBuffer);
                ArrayPool<T>.Shared.Return(array);
            }

            _buffer = ref Unsafe.NullRef<T>();
            BufferLength = 0;
            IsStackalloced = false;
            return;
        }

        T[] newBuffer = ArrayPool<T>.Shared.Rent(trimmedBufferSize);
        ref T source = ref MemoryMarshal.GetReference(oldBuffer);
        ref T destination = ref MemoryMarshal.GetArrayDataReference(newBuffer);
        SpanHelpers.Memmove(ref destination, ref source, (uint)count);

        if (!IsStackalloced)
        {
            T[] array = SpanMarshal.AsArray(oldBuffer);
            ArrayPool<T>.Shared.Return(array);
        }

        _buffer = ref MemoryMarshal.GetArrayDataReference(newBuffer);
        BufferLength = newBuffer.Length;
        IsStackalloced = false;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            AsSpan().Clear();
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
        if (IsDisposed)
        {
            ThrowHelper.ThrowObjectDisposedException<ValueList<T>>();
        }

        return ref _buffer;
    }

    private readonly Span<T> GetBuffer() => MemoryMarshal.CreateSpan(ref GetBufferReference(), BufferLength);

    public override readonly string ToString()
    {
        if (typeof(T) != typeof(char))
        {
            return ToStringHelpers.FormatCollection<ValueList<T>>(Count);
        }

        ref char reference = ref Unsafe.As<T, char>(ref GetBufferReference());
        return new(MemoryMarshal.CreateReadOnlySpan(ref reference, Count));
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
    public override readonly int GetHashCode() => Count.GetHashCode();

    public static bool operator ==(ValueList<T> left, ValueList<T> right) => left.Equals(right);

    public static bool operator !=(ValueList<T> left, ValueList<T> right) => !(left == right);
}
