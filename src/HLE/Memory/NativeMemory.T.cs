using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Collections;
using HLE.Text;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Memory;

[DebuggerDisplay("{ToString()}")]
public sealed unsafe partial class NativeMemory<T> :
    IDisposable,
    ICollection<T>,
    ICopyable<T>,
    IIndexable<T>,
    IReadOnlyCollection<T>,
    ISpanProvider<T>,
    ICollectionProvider<T>,
    IMemoryProvider<T>,
    IEquatable<NativeMemory<T>>
    where T : unmanaged, IEquatable<T>
{
    public ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.AsRef<T>(Pointer + index);
        }
    }

    T IIndexable<T>.this[int index] => this[index];

    T IIndexable<T>.this[Index index] => this[index];

    public ref T this[Index index] => ref this[index.GetOffset(Length)];

    public Span<T> this[Range range] => AsSpan(range);

    public T* Pointer
    {
        get
        {
            nuint memory = _memory;
            if (memory == 0)
            {
                ThrowHelper.ThrowObjectDisposedException<NativeMemory<T>>();
            }

            return (T*)memory;
        }
    }

    public int Length { get; }

    public ref T Reference => ref Unsafe.AsRef<T>(Pointer);

    int IReadOnlyCollection<T>.Count => Length;

    int ICollection<T>.Count => Length;

    int ICountable.Count => Length;

    bool ICollection<T>.IsReadOnly => false;

    private nuint _memory;

    public static NativeMemory<T> Empty { get; } = new();

    private NativeMemory()
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

        nuint byteCount = checked((uint)sizeof(T) * (nuint)(uint)length);
        T* memory = (T*)NativeMemory.AlignedAlloc(byteCount, (uint)sizeof(nuint));
        if (zeroed)
        {
            ClearMemory((byte*)memory, byteCount);
        }

        _memory = (nuint)memory;
    }

    ~NativeMemory() => DisposeCore();

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    private void DisposeCore()
    {
        nuint memory = Interlocked.Exchange(ref _memory, 0);
        if (memory == 0)
        {
            return;
        }

        Debug.Assert(MemoryHelpers.IsAligned((void*)memory, (uint)sizeof(nuint)));
        NativeMemory.AlignedFree((void*)memory);
    }

    private static void ClearMemory(byte* memory, nuint byteCount)
    {
        Debug.Assert(byteCount != 0);

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
    public Span<T> AsSpan() => new(Pointer, Length);

    [Pure]
    public Span<T> AsSpan(int start) => new Slicer<T>(Pointer, Length).SliceSpan(start);

    [Pure]
    public Span<T> AsSpan(int start, int length) => new Slicer<T>(Pointer, Length).SliceSpan(start, length);

    [Pure]
    public Span<T> AsSpan(Range range) => new Slicer<T>(Pointer, Length).SliceSpan(range);

    [Pure]
    public Memory<T> AsMemory() => new MemoryManager(this).Memory;

#pragma warning disable CA2000 // dispose NativeMemoryManager not called. That would free the memory.
    [Pure]
    public Memory<T> AsMemory(int start) => new MemoryManager(this).Memory[start..];

    [Pure]
    public Memory<T> AsMemory(int start, int length) => new MemoryManager(this).Memory.Slice(start, length);

    [Pure]
    public Memory<T> AsMemory(Range range) => new MemoryManager(this).Memory[range];
#pragma warning restore CA2000

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

    Memory<T> IMemoryProvider<T>.GetMemory() => AsMemory();

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.GetReadOnlyMemory() => AsMemory();

    public void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(ref destination);
    }

    public void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(Pointer, Length);
        copyWorker.CopyTo(destination);
    }

    [Pure]
    public T[] ToArray()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        ref T source = ref Reference;
        T[] result = GC.AllocateUninitializedArray<T>(length);
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(result), ref source, (uint)length);
        return result;
    }

    [Pure]
    public T[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public T[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public T[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<T> ToList() => AsSpan().ToList();

    [Pure]
    public List<T> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public List<T> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public List<T> ToList(Range range) => AsSpan().ToList(range);

    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    public void Clear() => ClearMemory((byte*)_memory, (uint)sizeof(T) * (uint)Length);

    bool ICollection<T>.Contains(T item) => AsSpan().Contains(item);

    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    public NativeMemoryEnumerator<T> GetEnumerator() => new((T*)_memory, Length);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<T>.Enumerator : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public override string ToString()
    {
        if (typeof(T) == typeof(char))
        {
            return Length == 0 ? string.Empty : new((char*)Pointer, 0, Length);
        }

        return ToStringHelpers.FormatCollection(this);
    }

    [Pure]
    public bool Equals(NativeMemory<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is NativeMemory<T> other && Equals(other);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(NativeMemory<T>? left, NativeMemory<T>? right) => Equals(left, right);

    public static bool operator !=(NativeMemory<T>? left, NativeMemory<T>? right) => !(left == right);
}
