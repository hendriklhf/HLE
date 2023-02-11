using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

/// <summary>
/// Represents an output sink consisting of buffers from an <see cref="ArrayPool{T}"/> into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of the stored elements.</typeparam>
public sealed class PoolBufferWriter<T> : IBufferWriter<T>, IDisposable
{
    /// <summary>
    /// A <see cref="Span{T}"/> view over the written elements.
    /// </summary>
    public Span<T> WrittenSpan => ((Span<T>)_buffer)[.._length];

    /// <summary>
    /// A <see cref="Memory{T}"/> view over the written elements.
    /// </summary>
    public Memory<T> WrittenMemory => ((Memory<T>)_buffer)[.._length];

    /// <summary>
    /// The amount of written elements.
    /// </summary>
    public int Length => _length;

    private T[] _buffer;
    private int _length;
    private readonly int _defaultElementGrowth;

    /// <summary>
    /// Creates a new <see cref="PoolBufferWriter{T}"/> with an initial buffer size and an element growth.
    /// </summary>
    /// <param name="initialSize">The initial buffer size.</param>
    /// <param name="defaultElementGrowth">The default element growth.</param>
    public PoolBufferWriter(int initialSize, int defaultElementGrowth)
    {
        _buffer = ArrayPool<T>.Shared.Rent(initialSize);
        _defaultElementGrowth = defaultElementGrowth;
    }

    /// <inheritdoc/>
    public void Advance(int count)
    {
        _length += count;
    }

    /// <inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return ((Memory<T>)_buffer)[_length..];
    }

    /// <inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return ((Span<T>)_buffer)[_length..];
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

        int freeSpace = _buffer.Length - _length;
        if (freeSpace >= sizeHint)
        {
            return;
        }

        int neededSpace = sizeHint - freeSpace;
        int elementGrowth = neededSpace > _defaultElementGrowth ? neededSpace << 1 : _defaultElementGrowth;
        Grow(elementGrowth);
    }

    /// <summary>
    /// Grows the buffer by the given element growth.
    /// </summary>
    /// <param name="elementGrowth">The element growth. If <paramref name="elementGrowth"/> is 0, the default element growth will be taken.</param>
    private void Grow(int elementGrowth = 0)
    {
        if (elementGrowth == 0)
        {
            elementGrowth = _defaultElementGrowth;
        }

        using RentedArray<T> oldBuffer = _buffer;
        _buffer = ArrayPool<T>.Shared.Rent(elementGrowth);
        CopyWrittenElementsIntoNewBuffer(oldBuffer);
    }

    private unsafe void CopyWrittenElementsIntoNewBuffer(T[] oldBuffer)
    {
        ref byte source = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(oldBuffer));
        ref byte destination = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(_buffer));
        Unsafe.CopyBlock(ref destination, ref source, (uint)(sizeof(T) * _length));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(_buffer);
    }
}
