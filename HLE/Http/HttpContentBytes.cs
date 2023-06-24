using System;
using System.Diagnostics;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Http;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Length = {Count}")]
public readonly struct HttpContentBytes : IDisposable, ICountable, IEquatable<HttpContentBytes>, IIndexAccessible<byte>
{
    public byte this[int index] => Span[index];

    public ReadOnlySpan<byte> Span => _contentBuffer[..Count];

    public ReadOnlyMemory<byte> Memory => _contentBuffer.Memory[..Count];

    public int Count { get; }

    private readonly RentedArray<byte> _contentBuffer = RentedArray<byte>.Empty;

    public static HttpContentBytes Empty => new();

    public HttpContentBytes()
    {
    }

    public HttpContentBytes(RentedArray<byte> contentBuffer, int contentLength)
    {
        _contentBuffer = contentBuffer;
        Count = contentLength;
    }

    public void Dispose()
    {
        _contentBuffer.Dispose();
    }

    public bool Equals(HttpContentBytes other)
    {
        return _contentBuffer == other._contentBuffer && Count == other.Count || _contentBuffer[..Count].SequenceEqual(other._contentBuffer[..other.Count]);
    }

    public override bool Equals(object? obj)
    {
        return obj is HttpContentBytes other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_contentBuffer, Count);
    }

    public static bool operator ==(HttpContentBytes left, HttpContentBytes right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HttpContentBytes left, HttpContentBytes right)
    {
        return !(left == right);
    }
}
