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
using HLE.Strings;

namespace HLE.Resources;

[DebuggerDisplay("{ToString()}")]
public readonly unsafe struct Resource(byte* resource, int length)
    : IEquatable<Resource>, ICollection<byte>, IReadOnlySpanProvider<byte>, ICountable, ICopyable<byte>, IIndexAccessible<byte>, IReadOnlyList<byte>,
        ICollectionProvider<byte>
{
    byte IReadOnlyList<byte>.this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return _resource[index];
        }
    }

    byte IIndexAccessible<byte>.this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return _resource[index];
        }
    }

    public int Length { get; } = length;

    int ICountable.Count => Length;

    int IReadOnlyCollection<byte>.Count => Length;

    int ICollection<byte>.Count => Length;

    bool ICollection<byte>.IsReadOnly => true;

    private readonly byte* _resource = resource;

    public static Resource Empty => [];

    public Resource() : this(null, 0)
    {
    }

    [Pure]
    public ReadOnlySpan<byte> AsSpan() => new(_resource, Length);

    // TODO: maybe cache MemoryManager
    [Pure]
    public ReadOnlyMemory<byte> AsMemory() => new NativeMemoryManager<byte>(_resource, Length).Memory;

    [Pure]
    public byte[] ToArray()
    {
        if (Length == 0)
        {
            return [];
        }

        byte[] result = GC.AllocateUninitializedArray<byte>(Length);
        ref byte destination = ref MemoryMarshal.GetArrayDataReference(result);
        ref byte source = ref Unsafe.AsRef<byte>(_resource);
        Unsafe.CopyBlock(ref destination, ref source, (uint)Length);
        return result;
    }

    [Pure]
    public List<byte> ToList()
    {
        if (Length == 0)
        {
            return [];
        }

        List<byte> result = new(Length);
        CopyWorker<byte> copyWorker = new(_resource, Length);
        copyWorker.CopyTo(result);
        return result;
    }

    public void CopyTo(List<byte> destination, int offset = 0)
    {
        CopyWorker<byte> copyWorker = new(_resource, Length);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(byte[] destination, int offset = 0)
    {
        CopyWorker<byte> copyWorker = new(_resource, Length);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<byte> destination)
    {
        CopyWorker<byte> copyWorker = new(_resource, Length);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<byte> destination)
    {
        CopyWorker<byte> copyWorker = new(_resource, Length);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref byte destination)
    {
        CopyWorker<byte> copyWorker = new(_resource, Length);
        copyWorker.CopyTo(ref destination);
    }

    public void CopyTo(byte* destination)
    {
        CopyWorker<byte> copyWorker = new(_resource, Length);
        copyWorker.CopyTo(destination);
    }

    ReadOnlySpan<byte> IReadOnlySpanProvider<byte>.GetReadOnlySpan() => AsSpan();

    void ICollection<byte>.Add(byte item) => throw new NotSupportedException();

    void ICollection<byte>.Clear() => throw new NotSupportedException();

    bool ICollection<byte>.Contains(byte item) => AsSpan().Contains(item);

    bool ICollection<byte>.Remove(byte item) => throw new NotSupportedException();

    public NativeMemoryEnumerator<byte> GetEnumerator() => new(_resource, Length);

    IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => GetEnumerator();

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
