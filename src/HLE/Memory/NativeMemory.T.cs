using System;
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

[DebuggerDisplay("{ToString()}")]
public unsafe partial struct NativeMemory<T> :
    IDisposable,
    ICollection<T>,
    ICopyable<T>,
    IBitwiseEquatable<NativeMemory<T>>,
    IIndexAccessible<T>,
    IReadOnlyCollection<T>,
    ISpanProvider<T>,
    ICollectionProvider<T>,
    IMemoryProvider<T>
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

    public readonly T* Pointer
    {
        get
        {
            ThrowIfDisposed();
            return _memory;
        }
    }

    public readonly int Length
    {
        get => _lengthAndIsDisposed.Integer;
        private init
        {
            Debug.Assert(value >= 0, "value needs to be >= 0 and should be validated by the ctor");
            _lengthAndIsDisposed.SetIntegerUnsafe(value);
        }
    }

    private bool IsDisposed
    {
        readonly get => _lengthAndIsDisposed.Bool;
        set => _lengthAndIsDisposed.Bool = value;
    }

    public readonly ref T Reference => ref Unsafe.AsRef<T>(Pointer);

    readonly int IReadOnlyCollection<T>.Count => Length;

    readonly int ICollection<T>.Count => Length;

    readonly int ICountable.Count => Length;

    readonly bool ICollection<T>.IsReadOnly => false;

    internal readonly T* _memory;

    private IntBoolUnion<int> _lengthAndIsDisposed;

    public static NativeMemory<T> Empty => new();

    public NativeMemory()
    {
    }

    [MustDisposeResource]
    public NativeMemory(int length, bool zeroed = true)
    {
        if (length == 0)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Length = length;
        IsDisposed = false;

        ulong byteCount = checked((uint)sizeof(T) * (ulong)length);
        if (!Environment.Is64BitProcess && byteCount > nuint.MaxValue)
        {
            ThrowAmountBytesExceed32BitIntegerRange();
        }

        T* memory = (T*)NativeMemory.AlignedAlloc((nuint)byteCount, (uint)sizeof(nuint));
        if (zeroed)
        {
            ClearMemory((byte*)memory, byteCount);
        }

        _memory = memory;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowAmountBytesExceed32BitIntegerRange() =>
        throw new InvalidOperationException("The amount of bytes (sizeof(T) * length) needing to be allocated, " +
                                            "exceed the address range of the 32-bit architecture.");

    private static void ClearMemory(byte* memory, ulong byteCount)
    {
        while (byteCount >= uint.MaxValue)
        {
            Unsafe.InitBlock(memory, 0, uint.MaxValue);
            byteCount -= uint.MaxValue;
            memory += uint.MaxValue;
        }

        if (byteCount != 0)
        {
            Unsafe.InitBlock(memory, 0, (uint)byteCount);
        }
    }

    [Pure]
    public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Reference, Length);

    [Pure]
    public readonly Span<T> AsSpan(int start) => new Slicer<T>(Pointer, Length).SliceSpan(start);

    [Pure]
    public readonly Span<T> AsSpan(int start, int length) => new Slicer<T>(Pointer, Length).SliceSpan(start, length);

    [Pure]
    public readonly Span<T> AsSpan(Range range) => new Slicer<T>(Pointer, Length).SliceSpan(range);

    [Pure]
    public readonly Memory<T> AsMemory() => new NativeMemoryManager<T>(Pointer, Length).Memory;

#pragma warning disable CA2000 // dispose NativeMemoryManager (not needed)
    [Pure]
    public readonly Memory<T> AsMemory(int start) => new NativeMemoryManager<T>(Pointer, Length).Memory[start..];

    [Pure]
    public readonly Memory<T> AsMemory(int start, int length) => new NativeMemoryManager<T>(Pointer, Length).Memory.Slice(start, length);

    [Pure]
    public readonly Memory<T> AsMemory(Range range) => new NativeMemoryManager<T>(Pointer, Length).Memory[range];
#pragma warning restore CA2000

    readonly Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    readonly ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

    readonly Memory<T> IMemoryProvider<T>.GetMemory() => AsMemory();

    readonly ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.GetReadOnlyMemory() => AsMemory();

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        Debug.Assert((nuint)_memory % (nuint)sizeof(nuint) == 0);
        NativeMemory.AlignedFree(_memory);
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
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        ref T source = ref Reference;
        T[] result = GC.AllocateUninitializedArray<T>(length);
        CopyWorker<T>.Copy(ref source, ref MemoryMarshal.GetArrayDataReference(result), (uint)length);
        return result;
    }

    [Pure]
    public readonly T[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public readonly T[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public readonly T[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public readonly List<T> ToList()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        T* source = Pointer;
        List<T> result = new(length);
        CopyWorker<T> copyWorker = new(source, length);
        copyWorker.CopyTo(result);
        return result;
    }

    readonly void ICollection<T>.Add(T item) => throw new NotSupportedException();

    public readonly void Clear() => ClearMemory((byte*)_memory, (uint)sizeof(T) * (ulong)Length);

    readonly bool ICollection<T>.Contains(T item) => AsSpan().Contains(item);

    readonly bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    private readonly void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            ThrowHelper.ThrowObjectDisposedException<NativeMemory<T>>();
        }
    }

    public readonly NativeMemoryEnumerator<T> GetEnumerator() => new(_memory, Length);

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public override readonly string ToString()
        => typeof(T) == typeof(char)
            ? Length == 0 ? string.Empty : new((char*)Pointer, 0, Length)
            : ToStringHelpers.FormatCollection(this);

    [Pure]
    public readonly bool Equals(NativeMemory<T> other) => Length == other.Length && Pointer == other.Pointer;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is NativeMemory<T> other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine((nuint)Pointer, Length);

    public static bool operator ==(NativeMemory<T> left, NativeMemory<T> right) => left.Equals(right);

    public static bool operator !=(NativeMemory<T> left, NativeMemory<T> right) => !(left == right);
}
