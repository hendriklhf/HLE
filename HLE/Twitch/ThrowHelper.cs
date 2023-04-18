using System;

namespace HLE.Twitch;

internal static class ThrowHelper
{
    public static InvalidOperationException NotConnected => new("The client is not connected.");

    public static InvalidOperationException AnonymousConnection => new("The client is connected anonymously.");

    public static InvalidOperationException NotConnectedToTheSpecifiedChannel => new("The client is not connected to the specified channel.");
}
