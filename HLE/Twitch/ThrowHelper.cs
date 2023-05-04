using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace HLE.Twitch;

internal static class ThrowHelper
{
    public static InvalidOperationException NotConnected => new("The client is not connected.");

    public static InvalidOperationException AnonymousConnection => new("The client is connected anonymously.");

    public static InvalidOperationException NotConnectedToTheSpecifiedChannel => new("The client is not connected to the specified channel.");

    public static InvalidOperationException EmptyHttpResponseContentBody => new("The request delivered a empty content.");

    [Pure]
    public static InvalidOperationException HttpRequestDidntSucceed(int statusCode, ReadOnlySpan<byte> content)
    {
        return new($"The request failed with code {statusCode} and delivered: {Encoding.UTF8.GetString(content)}");
    }
}
