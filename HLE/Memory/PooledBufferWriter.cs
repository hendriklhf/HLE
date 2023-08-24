using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Memory;

/// <summary>
/// Represents an output sink consisting of buffers from an <see cref="ArrayPool{T}"/> into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
[DebuggerDisplay("{ToString()}")]
public sealed class PooledBufferWriter<T> : IBufferWriter<T>, ICollection<T>, IDisposable, ICopyable<T>, ICountable, IEquatable<PooledBufferWriter<T>>, IIndexAccessible<T>, IReadOnlyCollection<T>
{
    T IIndexAccessible<T>.this[int index] => WrittenSpan[index];

    /// <summary>
    /// A <see cref="Span{T}"/> view over the written elements.
    /// </summary>
    public Span<T> WrittenSpan => _buffer[..Count];

    /// <summary>
    /// A <see cref="Memory{T}"/> view over the written elements.
    /// </summary>
    public Memory<T> WrittenMemory => _buffer.Memory[..Count];

    /// <summary>
    /// The amount of written elements.
    /// </summary>
    public int Count { get; internal set; }

    public int Capacity => _buffer.Length;

    int ICollection<T>.Count => Count;

    bool ICollection<T>.IsReadOnly => false;

    internal RentedArray<T> _buffer;

    private const int _defaultCapacity = 16;

    public PooledBufferWriter() : this(_defaultCapacity)
    {
    }

    public PooledBufferWriter(int capacity)
    {
        _buffer = new(capacity);
    }

    ~PooledBufferWriter()
    {
        _buffer.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _buffer.Dispose();
    }

    /// <inheritdoc/>
    public void Advance(int count)
    {
        Count += count;
    }

    /// <inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _buffer.Memory[Count..];
    }

    /// <inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _buffer[Count..];
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

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            WrittenSpan.Clear();
        }

        Count = 0;
    }

    public void EnsureCapacity(int capacity)
    {
        if (Capacity >= capacity)
        {
            return;
        }

        int neededSpace = capacity - Capacity;
        Grow(neededSpace);
    }

    [Pure]
    public T[] ToArray()
    {
        return WrittenSpan.ToArray();
    }

    [Pure]
    public List<T> ToList()
    {
        List<T> result = new(Count);
        CollectionsMarshal.SetCount(result, Count);
        Span<T> resultSpan = CollectionsMarshal.AsSpan(result);
        CopyTo(resultSpan);
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

        int neededSpace = sizeHint - freeSpace;
        bool neededSpaceLargerThanCurrentBuffer = neededSpace > _buffer.Length;
        int twiceTheNeededSpace = neededSpace << 1;
        int elementGrowth = neededSpaceLargerThanCurrentBuffer ? twiceTheNeededSpace : neededSpace;
        Grow(elementGrowth);
    }

    /// <summary>
    /// Grows the buffer by the given needed space.
    /// </summary>
    /// <param name="neededSpace">The needed space. If <paramref name="neededSpace"/> is 0, the default capacity will be taken.</param>
    private void Grow(int neededSpace = 0)
    {
        if (neededSpace == 0)
        {
            neededSpace = _buffer.Length == 0 ? _defaultCapacity : _buffer.Length;
        }

        using RentedArray<T> oldBuffer = _buffer;
        int newCapacity = _buffer.Length + neededSpace;
        _buffer = new(newCapacity);
        oldBuffer.CopyTo(_buffer.Span);
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(WrittenSpan);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(WrittenSpan);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        DefaultCopier<T> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        DefaultCopier<T> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        DefaultCopier<T> copier = new(WrittenSpan);
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        DefaultCopier<T> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    void ICollection<T>.Add(T item)
    {
        GetReference() = item;
        Advance(1);
    }

    bool ICollection<T>.Contains(T item)
    {
        return _buffer.Contains(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        int index = Array.IndexOf(_buffer._array, item);
        if (index < 0)
        {
            return false;
        }

        _buffer[(index + 1)..].CopyTo(_buffer[index..]);
        Advance(-1);
        return true;
    }

    [Pure]
    public bool Equals(PooledBufferWriter<T>? other)
    {
        return ReferenceEquals(this, other) || Count == other?.Count && _buffer.Equals(other._buffer);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is PooledBufferWriter<T> other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    [Pure]
    public override string ToString()
    {
        if (typeof(char) == typeof(T))
        {
            ref char charsReference = ref Unsafe.As<T, char>(ref _buffer.ManagedPointer);
            ReadOnlySpan<char> chars = MemoryMarshal.CreateReadOnlySpan(ref charsReference, Count);
            return new(chars);
        }

        Type thisType = typeof(PooledBufferWriter<T>);
        Type genericType = typeof(T);
        return $"{thisType.Name}.{nameof(PooledBufferWriter<T>)}<{genericType.Name}.{genericType.Name}>[{Count}]";
    }

    public IEnumerator<T> GetEnumerator()
    {
        RentedArray<T> buffer = _buffer;
        int length = Count;
        for (int i = 0; i < length; i++)
        {
            yield return buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
