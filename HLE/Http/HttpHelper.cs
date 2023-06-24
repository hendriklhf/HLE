using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE.Http;

public static class HttpHelper
{
    [Pure]
    public static int GetContentLength(this HttpResponseMessage httpResponse)
    {
        long contentLength = httpResponse.Content.Headers.ContentLength ?? 0;
        return contentLength switch
        {
            0 => throw new InvalidOperationException("The HTTP response content has a length of 0."),
            > int.MaxValue => throw new InvalidOperationException($"The HTTP response content exceed the size of {typeof(int)}."),
            _ => (int)contentLength
        };
    }

    [Pure]
    public static async ValueTask<HttpContentBytes> GetContentBytesAsync(this HttpResponseMessage httpResponse)
    {
        int contentLength = httpResponse.GetContentLength();
        if (contentLength == 0)
        {
            return HttpContentBytes.Empty;
        }

        return await GetContentBytesAsync(httpResponse, contentLength);
    }

    [Pure]
    public static async ValueTask<HttpContentBytes> GetContentBytesAsync(this HttpResponseMessage httpResponse, int contentLength)
    {
        Debug.Assert(contentLength > 0, "contentLength > 0");
        RentedArray<byte> memoryBuffer = new(contentLength);
        using MemoryStream memoryStream = new(memoryBuffer);
        await httpResponse.Content.LoadIntoBufferAsync(contentLength);
        await httpResponse.Content.CopyToAsync(memoryStream);
        return new(memoryBuffer, contentLength);
    }
}
