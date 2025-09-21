using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using HLE.Text;

namespace HLE.Resources;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{ToString()}")]
public readonly unsafe struct Resource(byte* resource, int length) :
    IBitwiseEquatable<Resource>,
    ICollection<byte>,
    IReadOnlySpanProvider<byte>,
    ICopyable<byte>,
    IIndexable<byte>,
    IReadOnlyList<byte>,
    ICollectionProvider<byte>,
    IReadOnlyMemoryProvider<byte>
{
    byte IReadOnlyList<byte>.this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return _resource[index];
        }
    }

    byte IIndexable<byte>.this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return _resource[index];
        }
    }

    byte IIndexable<byte>.this[Index index] => ((IIndexable<byte>)this)[index.GetOffset(Length)];

    public int Length { get; } = length;

    int ICountable.Count => Length;

    int IReadOnlyCollection<byte>.Count => Length;

    int ICollection<byte>.Count => Length;

    bool ICollection<byte>.IsReadOnly => true;

    private readonly byte* _resource = resource;

    [SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null")]
    public static Resource Empty => default;

    public Resource() : this(null, 0)
    {
    }

    [Pure]
    public ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(_resource), Length);

    [Pure]
    public ReadOnlySpan<byte> AsSpan(int start) => Slicer.SliceReadOnly(ref Unsafe.AsRef<byte>(_resource), Length, start);

    [Pure]
    public ReadOnlySpan<byte> AsSpan(int start, int length) => Slicer.SliceReadOnly(ref Unsafe.AsRef<byte>(_resource), Length, start, length);

    [Pure]
    public ReadOnlySpan<byte> AsSpan(Range range) => Slicer.SliceReadOnly(ref Unsafe.AsRef<byte>(_resource), Length, range);

    [Pure]
    public ReadOnlyMemory<byte> AsMemory() => new NativeMemoryManager<byte>(_resource, Length).Memory;

    [Pure]
    public ReadOnlyMemory<byte> AsMemory(int start) => AsMemory()[start..];

    [Pure]
    public ReadOnlyMemory<byte> AsMemory(int start, int length) => AsMemory().Slice(start, length);

    [Pure]
    public ReadOnlyMemory<byte> AsMemory(Range range) => AsMemory()[range];

    [Pure]
    public byte[] ToArray()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        byte[] result = GC.AllocateUninitializedArray<byte>(length);
        ref byte destination = ref MemoryMarshal.GetArrayDataReference(result);
        ref byte source = ref Unsafe.AsRef<byte>(_resource);
        SpanHelpers.Memmove(ref destination, ref source, length);
        return result;
    }

    [Pure]
    public byte[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public byte[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public byte[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<byte> ToList() => AsSpan().ToList();

    [Pure]
    public List<byte> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public List<byte> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public List<byte> ToList(Range range) => AsSpan().ToList(range);

    public void CopyTo(List<byte> destination, int offset = 0)
        => SpanHelpers.CopyChecked(AsSpan(), destination, offset);

    public void CopyTo(byte[] destination, int offset = 0)
        => SpanHelpers.CopyChecked(AsSpan(), destination.AsSpan(offset..));

    public void CopyTo(Memory<byte> destination) => SpanHelpers.CopyChecked(AsSpan(), destination.Span);

    public void CopyTo(Span<byte> destination) => SpanHelpers.CopyChecked(AsSpan(), destination);

    public void CopyTo(ref byte destination) => SpanHelpers.Copy(AsSpan(), ref destination);

    public void CopyTo(byte* destination) => SpanHelpers.Memmove(destination, _resource, Length);

    void ICollection<byte>.Add(byte item) => throw new NotSupportedException();

    void ICollection<byte>.Clear() => throw new NotSupportedException();

    bool ICollection<byte>.Contains(byte item) => AsSpan().Contains(item);

    bool ICollection<byte>.Remove(byte item) => throw new NotSupportedException();

    public NativeMemoryEnumerator<byte> GetEnumerator() => new(_resource, Length);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<byte>.Enumerator : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public override string ToString() => ToStringHelpers.FormatCollection(this);

    [Pure]
    public bool Equals(Resource other) => _resource == other._resource && Length == other.Length;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Resource other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine((nuint)_resource, Length);

    public static bool operator ==(Resource left, Resource right) => left.Equals(right);

    public static bool operator !=(Resource left, Resource right) => !(left == right);
}
