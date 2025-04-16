using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Twitch.Tmi.Models;

[DebuggerDisplay("{ToString()}")]
public struct Bytes : IDisposable, IEquatable<Bytes>, IReadOnlySpanProvider<byte>, IReadOnlyMemoryProvider<byte>, ICollectionProvider<byte>
{
    public readonly int Length => _length;

    private byte[]? _buffer;
    private readonly int _length;

    public static Bytes Empty => new();

    public Bytes()
    {
    }

    private Bytes(byte[] buffer, int length)
    {
        _buffer = buffer;
        _length = length;
    }

    public Bytes(ReadOnlySpan<byte> data)
    {
        _length = data.Length;
        _buffer = ArrayPool<byte>.Shared.Rent(data.Length);
        SpanHelpers.Copy(data, _buffer);
    }

    internal static Bytes AsBytes(byte[] buffer, int length)
    {
        Debug.Assert(buffer.Length >= length);
        return new(buffer, length);
    }

    public void Dispose()
    {
        byte[]? buffer = Interlocked.Exchange(ref _buffer, null);
        if (buffer is null)
        {
            return;
        }

        ArrayPool<byte>.Shared.Return(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly byte[] GetBuffer()
    {
        byte[]? buffer = _buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<Bytes>();
        }

        return buffer;
    }

    [Pure]
    public readonly ReadOnlySpan<byte> AsSpan() => GetBuffer().AsSpanUnsafe(0, _length);

    [Pure]
    public readonly ReadOnlySpan<byte> AsSpan(int start) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), _length, start);

    [Pure]
    public readonly ReadOnlySpan<byte> AsSpan(int start, int length) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), _length, start, length);

    [Pure]
    public readonly ReadOnlySpan<byte> AsSpan(Range range) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), _length, range);

    [Pure]
    public readonly ReadOnlyMemory<byte> AsMemory() => GetBuffer().AsMemory(0, _length);

    [Pure]
    public readonly ReadOnlyMemory<byte> AsMemory(int start) => AsMemory()[start..];

    [Pure]
    public readonly ReadOnlyMemory<byte> AsMemory(int start, int length) => AsMemory().Slice(start, length);

    [Pure]
    public readonly ReadOnlyMemory<byte> AsMemory(Range range) => AsMemory()[range];

    [Pure]
    public readonly byte[] ToArray()
    {
        int length = _length;
        if (length == 0)
        {
            return [];
        }

        byte[] result = GC.AllocateUninitializedArray<byte>(length);
        SpanHelpers.Copy(AsSpan(), result);
        return result;
    }

    [Pure]
    public readonly byte[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public readonly byte[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public readonly byte[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public readonly List<byte> ToList() => AsSpan().ToList();

    [Pure]
    public readonly List<byte> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public readonly List<byte> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public readonly List<byte> ToList(Range range) => AsSpan().ToList(range);

    public override readonly string ToString() => _length == 0 ? string.Empty : Encoding.UTF8.GetString(AsSpan());

    public readonly bool Equals(Bytes other) => _length == other._length && ReferenceEquals(_buffer, other._buffer);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bytes other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_buffer, _length);

    public static bool operator ==(Bytes left, Bytes right) => left.Equals(right);

    public static bool operator !=(Bytes left, Bytes right) => !(left == right);
}
