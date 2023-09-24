using System;
using System.Diagnostics;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public struct ReceivedData : IDisposable, IEquatable<ReceivedData>, ICountable, IIndexAccessible<char>
{
    public readonly char this[int index] => Span[index];

    public readonly ReadOnlySpan<char> Span => _data[..Length];

    public readonly ReadOnlyMemory<char> Memory => _data.AsMemory(..Length);

    public int Length { get; }

    readonly int ICountable.Count => Length;

    internal RentedArray<char> _data;

    public ReceivedData(ReadOnlySpan<char> data)
    {
        Length = data.Length;
        _data = ArrayPool<char>.Shared.CreateRentedArray(data.Length);
        data.CopyTo(_data.AsSpan());
    }

    public void Dispose()
    {
        _data.Dispose();
    }

    public readonly bool Equals(ReceivedData other)
    {
        return _data.Equals(other._data);
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj is ReceivedData other && Equals(other);
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => HashCode.Combine(_data, Length);

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString() => new(Span);

    public static bool operator ==(ReceivedData left, ReceivedData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ReceivedData left, ReceivedData right)
    {
        return !(left == right);
    }
}
