﻿using System;
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
        int contentLength = (int)(httpResponse.Content.Headers.ContentLength ?? 0);
        if (contentLength < 0)
        {
            throw new InvalidOperationException("The HTTP response content has a length of less than 0 or is null.");
        }

        return contentLength;
    }

    [Pure]
    public static async ValueTask<HttpContentBytes> GetContentBytes(this HttpResponseMessage httpResponse)
    {
        int contentLength = httpResponse.GetContentLength();
        if (contentLength == 0)
        {
            return HttpContentBytes.Empty;
        }

        return await GetContentBytes(httpResponse, contentLength);
    }

    internal static async ValueTask<HttpContentBytes> GetContentBytes(this HttpResponseMessage httpResponse, int contentLength)
    {
        RentedArray<byte> buffer = new(contentLength);
        using MemoryStream memoryStream = new(buffer);
        await httpResponse.Content.LoadIntoBufferAsync(contentLength);
        await httpResponse.Content.CopyToAsync(memoryStream);
        return new(buffer, contentLength);
    }
}
