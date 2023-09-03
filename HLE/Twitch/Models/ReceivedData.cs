using System;
using System.Diagnostics;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public readonly struct ReceivedData : IDisposable, IEquatable<ReceivedData>, ICountable, IIndexAccessible<char>
{
    public char this[int index] => Span[index];

    public ReadOnlySpan<char> Span => _data[..Length];

    public ReadOnlyMemory<char> Memory => _data.AsMemory()[..Length];

    public int Length { get; }

    int ICountable.Count => Length;

    internal readonly RentedArray<char> _data;

    public ReceivedData(ReadOnlySpan<char> data)
    {
        Length = data.Length;
        _data = new(Length);
        data.CopyTo(_data.AsSpan());
    }

    public void Dispose()
    {
        _data.Dispose();
    }

    public bool Equals(ReceivedData other)
    {
        return _data.Equals(other._data);
    }

    public override bool Equals(object? obj)
    {
        return obj is ReceivedData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_data, Length);
    }

    public override string ToString()
    {
        return new(Span);
    }

    public static bool operator ==(ReceivedData left, ReceivedData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ReceivedData left, ReceivedData right)
    {
        return !(left == right);
    }
}
