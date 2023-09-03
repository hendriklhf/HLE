using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Memory;

public readonly unsafe struct NativeMemory<T> : IDisposable, ICollection<T>, ICopyable<T>, IEquatable<NativeMemory<T>>, ICountable, IIndexAccessible<T>, IReadOnlyCollection<T>, ISpanProvider<T>
    where T : unmanaged, IEquatable<T>
{
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.AsRef<T>(_pointer + index);
        }
    }

    T IIndexAccessible<T>.this[int index] => this[index];

    public ref T this[Index index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            int actualIndex = index.GetOffset(Length);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(actualIndex, Length);
            return ref Unsafe.AsRef<T>(_pointer + actualIndex);
        }
    }

    public Span<T> this[Range range]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            int start = range.Start.GetOffset(Length);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, Length);

            int length = range.End.GetOffset(Length) - start;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(length, Length - start);

            return new(_pointer + start, length);
        }
    }

    public int Length { get; }

    int IReadOnlyCollection<T>.Count => Length;

    int ICollection<T>.Count => Length;

    int ICountable.Count => Length;

    bool ICollection<T>.IsReadOnly => false;

    internal readonly T* _pointer;

    public static NativeMemory<T> Empty => new();

    public NativeMemory()
    {
        _pointer = null;
        Length = 0;
    }

    public NativeMemory(int length)
    {
        _pointer = (T*)NativeMemory.AllocZeroed((nuint)(sizeof(T) * length));
        Length = length;
    }

    public NativeMemory(int length, bool zeroed)
    {
        nuint byteCount = (nuint)(sizeof(T) * length);
        _pointer = (T*)(zeroed ? NativeMemory.AllocZeroed(byteCount) : NativeMemory.Alloc(byteCount));
        Length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return new(_pointer, Length);
    }

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    public void Dispose()
    {
        NativeMemory.Free(_pointer);
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(ref destination);
    }

    public void CopyTo(T* destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    [Pure]
    public T[] ToArray()
    {
        T[] result = GC.AllocateUninitializedArray<T>(Length);
        ref byte destination = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(result));
        ref byte source = ref Unsafe.As<T, byte>(ref Unsafe.AsRef<T>(_pointer));
        Unsafe.CopyBlock(ref destination, ref source, (uint)(sizeof(T) * Length));
        return result;
    }

    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    public void Clear()
    {
        Unsafe.InitBlock(_pointer, 0, (uint)(sizeof(T) * Length));
    }

    bool ICollection<T>.Contains(T item)
    {
        return AsSpan().Contains(item);
    }

    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    public IEnumerator<T> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return AsSpan()[i];
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
            return new((char*)_pointer, 0, Length);
        }

        Type thisType = typeof(NativeMemory<T>);
        Type typeOfT = typeof(T);
        return $"{thisType.Namespace}.{thisType.Name.AsSpan(..^2)}<{typeOfT.Namespace}.{typeOfT.Name}>[{Length}]";
    }

    public bool Equals(NativeMemory<T> other)
    {
        return Length == other.Length && _pointer == other._pointer;
    }

    public override bool Equals(object? obj)
    {
        return obj is NativeMemory<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((nuint)_pointer, Length);
    }

    public static bool operator ==(NativeMemory<T> left, NativeMemory<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NativeMemory<T> left, NativeMemory<T> right)
    {
        return !(left == right);
    }
}
