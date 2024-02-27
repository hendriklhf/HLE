using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Twitch.Tmi.Models;

[DebuggerDisplay("{ToString()}")]
public struct Bytes : IDisposable, IEquatable<Bytes>, IReadOnlySpanProvider<byte>, IReadOnlyMemoryProvider<byte>, ICollectionProvider<byte>
{
    public int Length { get; }

    private RentedArray<byte> _buffer = [];

    public static Bytes Empty => new();

    public Bytes()
    {
    }

    internal Bytes(RentedArray<byte> buffer, int length)
    {
        _buffer = buffer;
        Length = length;
    }

    public Bytes(ReadOnlySpan<byte> data)
    {
        Length = data.Length;
        _buffer = ArrayPool<byte>.Shared.RentAsRentedArray(data.Length);
        SpanHelpers<byte>.Copy(data, _buffer.AsSpan());
    }

    public static Bytes AsBytes(RentedArray<byte> buffer, int length) => new(buffer, length);

    public void Dispose() => _buffer.Dispose();

    [Pure]
    public readonly ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref _buffer.Reference, Length);

    [Pure]
    public readonly ReadOnlyMemory<byte> AsMemory() => _buffer.AsMemory(..Length);

    [Pure]
    public readonly byte[] ToArray()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        byte[] result = GC.AllocateUninitializedArray<byte>(length);
        SpanHelpers<byte>.Copy(AsSpan(), result);
        return result;
    }

    [Pure]
    public readonly byte[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public readonly byte[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public readonly byte[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public readonly List<byte> ToList()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        List<byte> result = new(length);
        CopyWorker<byte> copyWorker = new(AsSpan());
        copyWorker.CopyTo(result);
        return result;
    }

    readonly ReadOnlySpan<byte> IReadOnlySpanProvider<byte>.GetReadOnlySpan() => AsSpan();

    readonly ReadOnlyMemory<byte> IReadOnlyMemoryProvider<byte>.GetReadOnlyMemory() => AsMemory();

    public override readonly string ToString() => Length == 0 ? string.Empty : Encoding.UTF8.GetString(AsSpan());

    public readonly bool Equals(Bytes other) => Length == other.Length && _buffer.Equals(other._buffer);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bytes other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_buffer, Length);

    public static bool operator ==(Bytes left, Bytes right) => left.Equals(right);

    public static bool operator !=(Bytes left, Bytes right) => !(left == right);
}
