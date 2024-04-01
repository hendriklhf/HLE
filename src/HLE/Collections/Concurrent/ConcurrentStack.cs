using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Memory;

namespace HLE.Collections.Concurrent;

/// <summary>
/// A concurrent stack that doesn't allocate on pushing items onto the stack, in comparision to <see cref="System.Collections.Concurrent.ConcurrentStack{T}"/>.
/// </summary>
/// <typeparam name="T">The type of stored items.</typeparam>
public sealed class ConcurrentStack<T> :
    IEquatable<ConcurrentStack<T>>,
    IReadOnlyCollection<T>,
    ICopyable<T>,
    IReadOnlySpanProvider<T>,
    IIndexable<T>,
    IMemoryProvider<T>,
    ICollectionProvider<T>
{
    T IIndexable<T>.this[int index] => AsSpan()[index];

    public int Count { get; private set; }

    public int Capacity => _buffer.Length;

    public object SyncRoot { get; } = new();

    internal T[] _buffer;

    private const int DefaultCapacity = 8;

    public ConcurrentStack() => _buffer = [];

    public ConcurrentStack(int capacity)
    {
        if (capacity < DefaultCapacity)
        {
            capacity = DefaultCapacity;
        }

        _buffer = GC.AllocateUninitializedArray<T>(capacity);
    }

    public ReadOnlySpan<T> AsSpan() => _buffer.AsSpanUnsafe(..Count);

    public void Push(T item)
    {
        lock (SyncRoot)
        {
            T[] buffer = _buffer;
            int count = Count;
            if (count == buffer.Length)
            {
                GrowBuffer();
            }

            ref T bufferReference = ref MemoryMarshal.GetArrayDataReference(buffer);
            ref T destination = ref Unsafe.Add(ref bufferReference, count);
            destination = item;
            Count = count + 1;
        }
    }

    public T Pop()
    {
        lock (SyncRoot)
        {
            int count = Count;
            if (count == 0)
            {
                ThrowStackIsEmpty();
            }

            int index = count - 1;
            ref T itemReference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), index);
            T item = itemReference;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                itemReference = default!;
            }

            Count = index;
            return item;
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStackIsEmpty() => throw new InvalidOperationException("The stack is empty.");

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        lock (SyncRoot)
        {
            int count = Count;
            if (count == 0)
            {
                item = default;
                return false;
            }

            int index = count - 1;
            ref T itemReference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), index);
            item = itemReference;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                itemReference = default!;
            }

            Count = index;
            return true;
        }
    }

    public void Clear()
    {
        lock (SyncRoot)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _buffer.AsSpan(0, Count).Clear();
            }

            Count = 0;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // slow path
    private void GrowBuffer()
    {
        Debug.Assert(Monitor.IsEntered(SyncRoot));

        int newBufferLength = BufferHelpers.GrowArray((uint)_buffer.Length, 1);
        T[] newBuffer = GC.AllocateUninitializedArray<T>(newBufferLength);
        SpanHelpers<T>.Copy(_buffer.AsSpan(0, Count), newBuffer);

        _buffer = newBuffer;
    }

    public List<T> ToList()
    {
        ReadOnlySpan<T> source = AsSpan();
        if (source.Length == 0)
        {
            return [];
        }

        List<T> result = new(source.Length);
        CopyWorker<T> copyWorker = new(source);
        copyWorker.CopyTo(result);
        return result;
    }

    public T[] ToArray()
    {
        ReadOnlySpan<T> source = AsSpan();
        if (source.Length == 0)
        {
            return [];
        }

        T[] result = GC.AllocateUninitializedArray<T>(source.Length);
        SpanHelpers<T>.Copy(source, result);
        return result;
    }

    public T[] ToArray(int start) => AsSpan().ToArray(start);

    public T[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    public T[] ToArray(Range range) => AsSpan().ToArray(range);

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

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.GetReadOnlyMemory() => _buffer.AsMemory(..Count);

    Memory<T> IMemoryProvider<T>.GetMemory() => _buffer.AsMemory(..Count);

    public ArrayEnumerator<T> GetEnumerator() => new(_buffer, 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] ConcurrentStack<T>? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ConcurrentStack<T>? left, ConcurrentStack<T>? right) => Equals(left, right);

    public static bool operator !=(ConcurrentStack<T>? left, ConcurrentStack<T>? right) => !(left == right);
}
