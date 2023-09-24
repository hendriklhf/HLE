using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Memory;

public unsafe struct NativeMemory<T> : IDisposable, ICollection<T>, ICopyable<T>, IEquatable<NativeMemory<T>>, ICountable, IIndexAccessible<T>, IReadOnlyCollection<T>, ISpanProvider<T>
    where T : unmanaged, IEquatable<T>
{
    public readonly ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.AsRef<T>(Pointer + index);
        }
    }

    readonly T IIndexAccessible<T>.this[int index] => this[index];

    public readonly ref T this[Index index] => ref this[index.GetOffset(Length)];

    public readonly Span<T> this[Range range] => AsSpan(range);

    public int Length
    {
        readonly get => (int)(_lengthAndDisposed & 0x7FFFFFFF);
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _lengthAndDisposed = (_lengthAndDisposed & 0x80000000) | (uint)value;
        }
    }

    internal readonly T* Pointer
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, typeof(NativeMemory<T>));
            return _pointer;
        }
    }

    internal bool IsDisposed
    {
        readonly get => (_lengthAndDisposed & 0x80000000) == 0x80000000;
        set
        {
            byte valueAsByte = Unsafe.As<bool, byte>(ref value);
            _lengthAndDisposed = (_lengthAndDisposed & 0x7FFFFFFF) | ((uint)valueAsByte << 31);
        }
    }

    public readonly ref T Reference => ref Unsafe.AsRef<T>(Pointer);

    readonly int IReadOnlyCollection<T>.Count => Length;

    readonly int ICollection<T>.Count => Length;

    readonly int ICountable.Count => Length;

    readonly bool ICollection<T>.IsReadOnly => false;

    private readonly T* _pointer;
    private uint _lengthAndDisposed;

    public static NativeMemory<T> Empty => new();

    public NativeMemory()
    {
        _pointer = null;
        Length = 0;
        IsDisposed = false;
    }

    public NativeMemory(int length, bool zeroed = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Length = length;
        IsDisposed = false;

        nuint byteCount = (nuint)(sizeof(T) * length);
        _pointer = (T*)(zeroed ? NativeMemory.AllocZeroed(byteCount) : NativeMemory.Alloc(byteCount));
    }

    [Pure]
    public readonly Span<T> AsSpan() => new(Pointer, Length);

    [Pure]
    public readonly Span<T> AsSpan(int start) => new Slicer<T>(Pointer, Length).CreateSpan(start);

    [Pure]
    public readonly Span<T> AsSpan(int start, int length) => new Slicer<T>(Pointer, Length).CreateSpan(start, length);

    [Pure]
    public readonly Span<T> AsSpan(Range range) => new Slicer<T>(Pointer, Length).CreateSpan(range);

    readonly Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        NativeMemory.Free(_pointer);
        IsDisposed = true;
    }

    public readonly void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(T[] destination, int offset)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(ref destination);
    }

    public readonly void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination);
    }

    [Pure]
    public readonly T[] ToArray()
    {
        T[] result = GC.AllocateUninitializedArray<T>(Length);
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(result);
        return result;
    }

    readonly void ICollection<T>.Add(T item) => throw new NotSupportedException();

    public readonly void Clear()
    {
        Unsafe.InitBlock(Pointer, 0, (uint)(sizeof(T) * Length));
    }

    readonly bool ICollection<T>.Contains(T item) => AsSpan().Contains(item);

    readonly bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    public readonly IEnumerator<T> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return Unsafe.Add(ref Reference, i);
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ReSharper disable once ArrangeModifiersOrder
    [Pure]
    public override readonly string ToString()
    {
        if (typeof(T) == typeof(char))
        {
            return new((char*)Pointer, 0, Length);
        }

        Type thisType = typeof(NativeMemory<T>);
        Type typeOfT = typeof(T);
        return $"{thisType.Namespace}.{thisType.Name.AsSpan(..^2)}<{typeOfT.Namespace}.{typeOfT.Name}>[{Length}]";
    }

    [Pure]
    public readonly bool Equals(NativeMemory<T> other)
    {
        return Length == other.Length && Pointer == other.Pointer;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj is NativeMemory<T> other && Equals(other);
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode()
    {
        return HashCode.Combine((nuint)Pointer, Length);
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
