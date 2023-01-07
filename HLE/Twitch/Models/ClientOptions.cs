namespace HLE.Twitch.Models;

/// <summary>
/// Options for <see cref="TwitchClient"/>. By default:<br/>
/// <see cref="ClientType"/> = <see cref="Models.ClientType.WebSocket"/><br/>
/// <see cref="UseSSL"/> = false<br/>
/// <see cref="IsVerifiedBot"/> = false<br/>
/// </summary>
public readonly struct ClientOptions
{
    /// <summary>
    /// The client type. Can be either a websocket or TCP connection.
    /// </summary>
    public ClientType ClientType { get; init; } = ClientType.WebSocket;

    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL { get; init; } = false;

    /// <summary>
    /// Indicates whether the bot is verified or not. If your bot is verified you can set this to true. Verified bots have higher rate limits.
    /// </summary>
    public bool IsVerifiedBot { get; init; } = false;

    public ClientOptions()
    {
    }
}
