using System;
using System.Collections.Immutable;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace HLE.Twitch;

public sealed class HttpRequestFailedException(int statusCode, ReadOnlySpan<byte> responseBytes) : Exception
{
    public HttpStatusCode HttpStatusCode { get; } = (HttpStatusCode)statusCode;

    public ImmutableArray<byte> HttpResponseContent { get; } = ImmutableCollectionsMarshal.AsImmutableArray(responseBytes.ToArray());

    public override string Message => _message ??= $"The request failed with code {HttpStatusCode} and delivered: {Encoding.UTF8.GetString(HttpResponseContent.AsSpan())}";

    private string? _message;

    public HttpRequestFailedException(HttpStatusCode statusCode, ReadOnlySpan<byte> responseBytes) : this((int)statusCode, responseBytes)
    {
    }
}
