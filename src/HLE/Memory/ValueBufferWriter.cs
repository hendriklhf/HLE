using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

/// <summary>
/// Represents an output sink consisting of a buffer from an <see cref="ArrayPool{T}"/> into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
[DebuggerDisplay("{ToString()}")]
public ref struct ValueBufferWriter<T> :
    IBufferWriter<T>,
    ICollection<T>,
    IDisposable,
    ICopyable<T>,
    IEquatable<ValueBufferWriter<T>>,
    IIndexable<T>,
    IReadOnlyCollection<T>,
    ISpanProvider<T>,
    ICollectionProvider<T>
{
    readonly T IIndexable<T>.this[int index] => WrittenSpan[index];

    readonly T IIndexable<T>.this[Index index] => WrittenSpan[index];

    /// <summary>
    /// A <see cref="Span{T}"/> view over the written elements.
    /// </summary>
    public readonly Span<T> WrittenSpan => GetBuffer().SliceUnsafe(..Count);

    /// <summary>
    /// The amount of written elements.
    /// </summary>
    public int Count
    {
        readonly get => _countAndIsDisposed.Integer;
        internal set
        {
            Debug.Assert(value >= 0);
            _countAndIsDisposed.SetIntegerUnsafe(value);
        }
    }

    private bool IsDisposed
    {
        readonly get => _countAndIsDisposed.Bool;
        set => _countAndIsDisposed.Bool = value;
    }

    private int BufferLength
    {
        readonly get => _bufferLengthAndIsStackalloced.Integer;
        set
        {
            Debug.Assert(value >= 0);
            _bufferLengthAndIsStackalloced.SetIntegerUnsafe(value);
        }
    }

    private bool IsStackalloced
    {
        readonly get => _bufferLengthAndIsStackalloced.Bool;
        set => _bufferLengthAndIsStackalloced.Bool = value;
    }

    public readonly int Capacity => GetBuffer().Length;

    readonly bool ICollection<T>.IsReadOnly => false;

    private ref T _buffer;
    private IntBoolUnion<int> _bufferLengthAndIsStackalloced;
    private IntBoolUnion<int> _countAndIsDisposed;

    public ValueBufferWriter()
    {
        _buffer = ref Unsafe.NullRef<T>();
        IsStackalloced = true;
    }

    public ValueBufferWriter(Span<T> buffer)
    {
        _buffer = ref MemoryMarshal.GetReference(buffer);
        BufferLength = buffer.Length;
        IsStackalloced = true;
    }

    public ValueBufferWriter(int capacity)
    {
        T[] buffer = ArrayPool<T>.Shared.Rent(capacity);
        _buffer = ref MemoryMarshal.GetArrayDataReference(buffer);
        BufferLength = buffer.Length;
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

    public void Advance(int count) => Count += count;

    public Span<T> GetSpan(int sizeHint = 0) => MemoryMarshal.CreateSpan(ref GetReference(sizeHint), Capacity - Count);

    /// <summary>
    /// Returns a reference to the buffer to write the requested size (specified by sizeHint) to.
    /// </summary>
    /// <param name="sizeHint">The minimum size of free buffer space after the given reference. If 0, at least one space will be available.</param>
    /// <returns>A reference to the buffer that can be written to.</returns>
    public ref T GetReference(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return ref Unsafe.Add(ref GetBufferReference(), Count);
    }

    readonly Memory<T> IBufferWriter<T>.GetMemory(int sizeHint) => throw new NotSupportedException();

    [Pure]
    public readonly Span<T> AsSpan() => GetBuffer().SliceUnsafe(0, Count);

    [Pure]
    public readonly Span<T> AsSpan(int start) => Slicer.Slice(ref GetBufferReference(), Count, start);

    [Pure]
    public readonly Span<T> AsSpan(int start, int length) => Slicer.Slice(ref GetBufferReference(), Count, start, length);

    [Pure]
    public readonly Span<T> AsSpan(Range range) => Slicer.Slice(ref GetBufferReference(), Count, range);

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

        if (data is ICopyable<T> copyable)
        {
            ref T destination = ref GetReference(copyable.Count);
            copyable.CopyTo(ref destination);
            Count += copyable.Count;
            return;
        }

        foreach (T item in data)
        {
            Write(item);
        }
    }

    public void Write(List<T> data) => Write(CollectionsMarshal.AsSpan(data));

    public void Write(T[] data) => Write(ref MemoryMarshal.GetArrayDataReference(data), data.Length);

    public void Write(scoped Span<T> data) => Write(ref MemoryMarshal.GetReference(data), data.Length);

    public void Write(scoped ReadOnlySpan<T> data) => Write(ref MemoryMarshal.GetReference(data), data.Length);

    public void Write(scoped ref T data, int length)
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

    void ICollection<T>.Add(T item) => Write(item);

    readonly bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    readonly bool ICollection<T>.Contains(T item) => throw new NotSupportedException();

    [Pure]
    public readonly T[] ToArray()
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
    public readonly T[] ToArray(int start) => WrittenSpan.ToArray(start);

    [Pure]
    public readonly T[] ToArray(int start, int length) => WrittenSpan.ToArray(start, length);

    [Pure]
    public readonly T[] ToArray(Range range) => WrittenSpan.ToArray(range);

    [Pure]
    public readonly List<T> ToList() => WrittenSpan.ToList();

    [Pure]
    public readonly List<T> ToList(int start) => WrittenSpan.ToList(start);

    [Pure]
    public readonly List<T> ToList(int start, int length) => WrittenSpan.ToList(start, length);

    [Pure]
    public readonly List<T> ToList(Range range) => WrittenSpan.ToList(range);

    /// <summary>
    /// Trims unused buffer size.<br/>
    /// This method should ideally be called, when <see cref="Capacity"/> is much larger than <see cref="Count"/>.
    /// </summary>
    /// <example>
    /// After having removed a lot of items from the <see cref="ValueBufferWriter{T}"/> <see cref="Capacity"/> will be much larger than <see cref="Count"/>.
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

        if (trimmedBufferSize == 0)
        {
            _buffer = ref Unsafe.NullRef<T>();
            BufferLength = 0;
            IsStackalloced = true;
            return;
        }

        Span<T> oldBuffer = GetBuffer();
        Span<T> newBuffer = ArrayPool<T>.Shared.Rent(trimmedBufferSize);
        SpanHelpers.Copy(oldBuffer[..count], newBuffer);
        _buffer = ref MemoryMarshal.GetReference(newBuffer);
        BufferLength = newBuffer.Length;

        if (IsStackalloced)
        {
            IsStackalloced = false;
            return;
        }

        T[] array = SpanMarshal.AsArray(oldBuffer);
        ArrayPool<T>.Shared.Return(array);
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

        int count = Count;
        Span<T> oldBuffer = GetBuffer();
        int newBufferSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        Span<T> newBuffer = ArrayPool<T>.Shared.Rent(newBufferSize);
        if (count != 0)
        {
            SpanHelpers.Copy(oldBuffer[..count], newBuffer);
        }

        _buffer = ref MemoryMarshal.GetReference(newBuffer);
        BufferLength = newBuffer.Length;

        if (IsStackalloced)
        {
            IsStackalloced = false;
            return;
        }

        T[] array = SpanMarshal.AsArray(oldBuffer);
        ArrayPool<T>.Shared.Return(array);
    }

    public readonly void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(scoped Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(scoped ref T destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(ref destination);
    }

    public readonly unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref T GetBufferReference()
    {
        if (IsDisposed)
        {
            ThrowHelper.ThrowObjectDisposedException<ValueBufferWriter<T>>();
        }

        return ref _buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly Span<T> GetBuffer() => MemoryMarshal.CreateSpan(ref GetBufferReference(), BufferLength);

    [Pure]
    public override readonly string ToString()
    {
        if (typeof(char) != typeof(T))
        {
            return ToStringHelpers.FormatCollection<ValueBufferWriter<T>>(Count);
        }

        ref char reference = ref Unsafe.As<T, char>(ref GetBufferReference());
        return new(MemoryMarshal.CreateReadOnlySpan(ref reference, Count));
    }

    public readonly MemoryEnumerator<T> GetEnumerator() => new(WrittenSpan);

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException();

    readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    [Pure]
    public readonly bool Equals(scoped ValueBufferWriter<T> other) => Count == other.Count && GetBuffer() == other.GetBuffer();

    [Pure]
    public override readonly bool Equals(object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => Count.GetHashCode();

    public static bool operator ==(ValueBufferWriter<T> left, ValueBufferWriter<T> right) => left.Equals(right);

    public static bool operator !=(ValueBufferWriter<T> left, ValueBufferWriter<T> right) => !(left == right);
}
