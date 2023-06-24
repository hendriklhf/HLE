using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Memory;

public readonly unsafe struct NativeMemory<T> : IDisposable, ICollection<T>, ICopyable<T>, IEquatable<NativeMemory<T>>, ICountable, IRefIndexAccessible<T>
    where T : unmanaged
{
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref _buffer[index];
        }
    }

    public ref T this[Index index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            int actualIndex = index.GetOffset(Length);
            if (actualIndex < 0 || actualIndex >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref _buffer[actualIndex];
        }
    }

    public Span<T> this[Range range]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            int start = range.Start.GetOffset(Length);
            int length = range.End.GetOffset(Length) - start;

            if (start < 0 || start >= Length || length >= Length - start)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            return new(_buffer + start, length);
        }
    }

    public Span<T> Span => new(_buffer, Length);

    public int Length { get; }

    int ICollection<T>.Count => Length;

    int ICountable.Count => Length;

    bool ICollection<T>.IsReadOnly => false;

    private readonly T* _buffer;

    public static NativeMemory<T> Empty => new();

    public NativeMemory()
    {
        _buffer = null;
        Length = 0;
    }

    public NativeMemory(int length)
    {
        _buffer = (T*)NativeMemory.AllocZeroed((nuint)(sizeof(T) * length));
        Length = length;
    }

    public void Dispose()
    {
        NativeMemory.Free(_buffer);
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(ref destination);
    }

    public void CopyTo(T* destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination);
    }

    [Pure]
    public T[] ToArray()
    {
        return Span.ToArray();
    }

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException();
    }

    public void Clear()
    {
        NativeMemory.Clear(_buffer, (nuint)(Length * sizeof(T)));
    }

    bool ICollection<T>.Contains(T item)
    {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return Span[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        if (typeof(T) == typeof(char))
        {
            return new((char*)_buffer, 0, Length);
        }

        Type thisType = typeof(NativeMemory<T>);
        Type typeOfT = typeof(T);
        return $"{thisType.Namespace}.{thisType.Name.AsSpan(..^2)}<{typeOfT.Namespace}.{typeOfT.Name}>[{Length}]";
    }

    public bool Equals(NativeMemory<T> other)
    {
        return Length == other.Length && _buffer == other._buffer;
    }

    public override bool Equals(object? obj)
    {
        return obj is NativeMemory<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((nuint)_buffer, Length);
    }

    public static bool operator ==(NativeMemory<T> left, NativeMemory<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NativeMemory<T> left, NativeMemory<T> right)
    {
        return !(left == right);
    }

    public static implicit operator Span<T>(NativeMemory<T> nativeMemory)
    {
        return nativeMemory.Span;
    }
}
