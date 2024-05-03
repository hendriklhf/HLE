using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Twitch;

internal struct HttpContentBytes : IEquatable<HttpContentBytes>, IDisposable
{
    public int Length { get; }

    private byte[]? _bytes = [];

    public static HttpContentBytes Empty => new();

    public HttpContentBytes()
    {
    }

    private HttpContentBytes(byte[] bytes, int length)
    {
        _bytes = bytes;
        Length = length;
    }

    public void Dispose()
    {
        byte[]? bytes = _bytes;
        if (bytes is null)
        {
            return;
        }

        ArrayPool<byte>.Shared.Return(bytes);
        _bytes = null;
    }

    public readonly ReadOnlySpan<byte> AsSpan() => GetBytes().AsSpanUnsafe(..Length);

    public readonly ReadOnlyMemory<byte> AsMemory() => GetBytes().AsMemory(..Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly byte[] GetBytes()
    {
        byte[]? bytes = _bytes;
        if (bytes is null)
        {
            ThrowHelper.ThrowObjectDisposedException<HttpContentBytes>();
        }

        return bytes;
    }

    public static ValueTask<HttpContentBytes> CreateAsync(HttpResponseMessage httpResponse)
    {
        long contentLength = httpResponse.Content.Headers.ContentLength ?? 0;
        if (contentLength == 0)
        {
            return ValueTask.FromResult(Empty);
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(contentLength, Array.MaxLength);

        return CreateCoreAsync(httpResponse.Content, (int)contentLength);

        // ReSharper disable once InconsistentNaming
        static async ValueTask<HttpContentBytes> CreateCoreAsync(HttpContent content, int contentLength)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(contentLength);
            await using MemoryStream copyDestination = new(buffer);
            await content.CopyToAsync(copyDestination);
            return new(buffer, contentLength);
        }
    }

    public readonly bool Equals(HttpContentBytes other) => Length == other.Length && _bytes == other._bytes;

    public override readonly bool Equals(object? obj) => obj is HttpContentBytes other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_bytes, Length);

    public static bool operator ==(HttpContentBytes left, HttpContentBytes right) => left.Equals(right);

    public static bool operator !=(HttpContentBytes left, HttpContentBytes right) => !(left == right);
}
