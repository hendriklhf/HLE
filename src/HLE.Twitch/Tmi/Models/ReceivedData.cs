using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Twitch.Tmi.Models;

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
        _data = ArrayPool<char>.Shared.RentAsRentedArray(data.Length);
        data.CopyTo(_data.AsSpan());
    }

    public void Dispose() => _data.Dispose();

    public readonly bool Equals(ReceivedData other) => _data.Equals(other._data);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is ReceivedData other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_data, Length);

    public override readonly string ToString() => Length == 0 ? string.Empty : new(Span);

    public static bool operator ==(ReceivedData left, ReceivedData right) => left.Equals(right);

    public static bool operator !=(ReceivedData left, ReceivedData right) => !(left == right);
}
