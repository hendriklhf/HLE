using System;
using HLE.Memory;

namespace HLE.Http;

public readonly struct HttpContentBytes : IDisposable, IEquatable<HttpContentBytes>
{
    public ReadOnlySpan<byte> Span => _contentBuffer[.._contentLength];

    public ReadOnlyMemory<byte> Memory => _contentBuffer.Memory[.._contentLength];

    private readonly RentedArray<byte> _contentBuffer = RentedArray<byte>.Empty;
    private readonly int _contentLength;

    public static HttpContentBytes Empty => new();

    public HttpContentBytes()
    {
    }

    public HttpContentBytes(RentedArray<byte> contentBuffer, int contentLength)
    {
        _contentBuffer = contentBuffer;
        _contentLength = contentLength;
    }

    public void Dispose()
    {
        _contentBuffer.Dispose();
    }

    public bool Equals(HttpContentBytes other)
    {
        return _contentBuffer == other._contentBuffer && _contentLength == other._contentLength || _contentBuffer[.._contentLength].SequenceEqual(other._contentBuffer[..other._contentLength]);
    }

    public override bool Equals(object? obj)
    {
        return obj is HttpContentBytes other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_contentBuffer, _contentLength);
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
