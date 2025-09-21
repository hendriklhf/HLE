using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Collections;
using HLE.Text;

namespace HLE.Memory;

[DebuggerDisplay("{ToString()}")]
public sealed unsafe partial class NativeMemory<T> :
    SafeHandle,
    ICollection<T>,
    ICopyable<T>,
    IIndexable<T>,
    IReadOnlyCollection<T>,
    ISpanProvider<T>,
    IReadOnlySpanProvider<T>,
    IMemoryProvider<T>,
    IReadOnlyMemoryProvider<T>,
    ICollectionProvider<T>,
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
            nint memory = handle;
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

    public override bool IsInvalid => handle == 0;

    bool ICollection<T>.IsReadOnly => false;

    public static NativeMemory<T> Empty { get; } = new(0, 0, false);

    private NativeMemory(nint memory, int length, bool ownsHandle) : base(memory, ownsHandle)
        => Length = length;

    [Pure]
    public static NativeMemory<T> Alloc(int length, bool zeroed = true)
    {
        if (length == 0)
        {
            return Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        nuint byteCount = checked((uint)sizeof(T) * (nuint)(uint)length);
        T* memory = (T*)NativeMemory.AlignedAlloc(byteCount, (nuint)sizeof(T));

        if (zeroed)
        {
            SpanHelpers.Clear(memory, length);
        }

        return new((nint)memory, length, true);
    }

    protected override bool ReleaseHandle()
    {
        DisposeCore();
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeCore();
        }

        base.Dispose(disposing);
    }

    private void DisposeCore()
    {
        nint memory = Interlocked.Exchange(ref handle, 0);
        if (memory == 0)
        {
            return;
        }

        Debug.Assert(MemoryHelpers.IsAligned((void*)memory, (nuint)sizeof(T)));
        NativeMemory.AlignedFree((void*)memory);
    }

    [Pure]
    public Span<T> AsSpan() => new(Pointer, Length);

    [Pure]
    public Span<T> AsSpan(int start) => Slicer.Slice(ref Unsafe.AsRef<T>(Pointer), Length, start);

    [Pure]
    public Span<T> AsSpan(int start, int length) => Slicer.Slice(ref Unsafe.AsRef<T>(Pointer), Length, start, length);

    [Pure]
    public Span<T> AsSpan(Range range) => Slicer.Slice(ref Unsafe.AsRef<T>(Pointer), Length, range);

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan() => AsSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(int start) => AsSpan(start..);

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(int start, int length) => AsSpan(start, length);

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.AsSpan(Range range) => AsSpan(range);

#pragma warning disable CA2000 // disposing the memory manager would free the memory
    [Pure]
    public Memory<T> AsMemory() => new MemoryManager(this).Memory;

    [Pure]
    public Memory<T> AsMemory(int start) => new MemoryManager(this).Memory[start..];

    [Pure]
    public Memory<T> AsMemory(int start, int length) => new MemoryManager(this).Memory.Slice(start, length);

    [Pure]
    public Memory<T> AsMemory(Range range) => new MemoryManager(this).Memory[range];
#pragma warning restore CA2000

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory() => AsMemory();

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory(int start) => AsMemory(start..);

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory(int start, int length) => AsMemory(start, length);

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.AsMemory(Range range) => AsMemory(range);

    public void CopyTo(List<T> destination, int offset = 0)
        => SpanHelpers.CopyChecked(AsSpan(), destination, offset);

    public void CopyTo(T[] destination, int offset)
        => SpanHelpers.CopyChecked(AsSpan(), destination.AsSpan(offset..));

    public void CopyTo(Memory<T> destination) => SpanHelpers.Copy(AsSpan(), destination.Span);

    public void CopyTo(Span<T> destination) => SpanHelpers.CopyChecked(AsSpan(), destination);

    public void CopyTo(ref T destination) => SpanHelpers.Copy(AsSpan(), ref destination);

    public void CopyTo(T* destination) => SpanHelpers.Copy(AsSpan(), destination);

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
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(result), ref source, length);
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

    public void Clear() => SpanHelpers.Clear(Pointer, Length);

    bool ICollection<T>.Contains(T item) => AsSpan().Contains(item);

    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    public NativeMemoryEnumerator<T> GetEnumerator() => new(Pointer, Length);

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
