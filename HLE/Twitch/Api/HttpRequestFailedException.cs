using System;
using System.Net;

namespace HLE.Twitch.Api;

public sealed class HttpRequestFailedException : Exception
{
    public HttpRequestFailedException(HttpStatusCode statusCode, string responseBody) : this((int)statusCode, responseBody)
    {
    }

    public HttpRequestFailedException(int statusCode, string responseBody)
        : base($"The request failed with code {statusCode} and delivered: {responseBody}")
    {
    }
}
