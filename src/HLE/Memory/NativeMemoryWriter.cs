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
/// Represents an output sink consisting of native memory into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
/// <param name="capacity">The starting capacity of the buffer.</param>
[method: MustDisposeResource]
[DebuggerDisplay("{ToString()}")]
public sealed unsafe class NativeBufferWriter<T>(int capacity) :
    IBufferWriter<T>,
    ICollection<T>,
    IDisposable,
    ICopyable<T>,
    IEquatable<NativeBufferWriter<T>>,
    IIndexAccessible<T>,
    IReadOnlyCollection<T>,
    ISpanProvider<T>,
    ICollectionProvider<T>,
    IMemoryProvider<T>
    where T : unmanaged, IEquatable<T>
{
    T IIndexAccessible<T>.this[int index] => WrittenSpan[index];

    /// <summary>
    /// A <see cref="Span{T}"/> view over the written elements.
    /// </summary>
    public Span<T> WrittenSpan => MemoryMarshal.CreateSpan(ref _buffer.Reference, Count);

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
    internal NativeMemory<T> _buffer = capacity == 0 ? [] : new(capacity, false);

    [MustDisposeResource]
    public NativeBufferWriter() : this(0)
    {
    }

    [MustDisposeResource]
    public NativeBufferWriter(ReadOnlySpan<T> data) : this(data.Length)
    {
        CopyWorker<T>.Copy(data, _buffer._memory);
        Count = data.Length;
    }

    public void Dispose() => _buffer.Dispose();

    /// <inheritdoc/>
    public void Advance(int count) => Count += count;

    /// <inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _buffer.AsMemory()[Count..];
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
        return ref Unsafe.Add(ref _buffer.Reference, Count);
    }

    public T* GetPointer(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _buffer.Pointer + Count;
    }

    public void Write(T item)
    {
        *GetPointer() = item;
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
            T* destination = GetPointer(copyable.Count);
            copyable.CopyTo(destination);
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

    public void Write(Span<T> data) => Write(ref MemoryMarshal.GetReference(data), data.Length);

    public void Write(ReadOnlySpan<T> data) => Write(ref MemoryMarshal.GetReference(data), data.Length);

    public void Write(ref T data, int length)
    {
        ref T destination = ref GetReference(length);
        CopyWorker<T>.Copy(ref data, ref destination, (uint)length);
        Count += length;
    }

    public void Write(T* data, int length) => Write(ref Unsafe.AsRef<T>(data), length);

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
        CopyWorker<T>.Copy(writtenSpan, result);
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

    public void TrimBuffer()
    {
        int count = Count;
        int trimmedBufferSize = BufferHelpers.GrowBuffer((uint)count, 0);
        if (trimmedBufferSize == Capacity)
        {
            return;
        }

        using NativeMemory<T> oldBuffer = _buffer;
        if (trimmedBufferSize == 0)
        {
            _buffer = [];
            return;
        }

        T* source = oldBuffer.Pointer;
        NativeMemory<T> newBuffer = new(trimmedBufferSize, false);
        CopyWorker<T>.Copy(source, newBuffer.Pointer, (uint)count);
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
        Debug.Assert(neededSize >= 0);

        using NativeMemory<T> oldBuffer = _buffer;
        T* source = _buffer.Pointer;

        int newBufferSize = BufferHelpers.GrowBuffer((uint)oldBuffer.Length, (uint)neededSize);
        NativeMemory<T> newBuffer = new(newBufferSize, false);
        int count = Count;
        if (count != 0)
        {
            CopyWorker<T>.Copy(source, newBuffer.Pointer, (uint)count);
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

    public void CopyTo(T* destination)
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

    [Pure]
    public override string ToString()
        => typeof(char) == typeof(T)
            ? Unsafe.As<NativeMemory<T>, NativeMemory<char>>(ref _buffer).AsSpan(..Count).ToString()
            : ToStringHelpers.FormatCollection(this);

    public NativeMemoryEnumerator<T> GetEnumerator() => new(_buffer.Pointer, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] NativeBufferWriter<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(NativeBufferWriter<T>? left, NativeBufferWriter<T>? right) => Equals(left, right);

    public static bool operator !=(NativeBufferWriter<T>? left, NativeBufferWriter<T>? right) => !(left == right);
}
