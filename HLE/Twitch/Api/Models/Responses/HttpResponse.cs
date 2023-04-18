using System;
using HLE.Memory;

namespace HLE.Twitch.Api.Models.Responses;

internal readonly struct HttpResponse : IDisposable
{
    public ReadOnlyMemory<byte> Bytes => _httpContentBuffer.Memory[.._contentLength];

    private readonly RentedArray<byte> _httpContentBuffer;
    private readonly int _contentLength;

    public HttpResponse(RentedArray<byte> httpContentBuffer, int contentLength)
    {
        _httpContentBuffer = httpContentBuffer;
        _contentLength = contentLength;
    }

    public void Dispose()
    {
        _httpContentBuffer.Dispose();
    }
}
