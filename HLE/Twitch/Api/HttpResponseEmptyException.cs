using System;

namespace HLE.Twitch.Api;

public sealed class HttpResponseEmptyException : Exception
{
    public HttpResponseEmptyException() : base("The HTTP response contains an empty body.")
    {
    }
}
