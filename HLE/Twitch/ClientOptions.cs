using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// Options for <see cref="TwitchClient"/>. By default:<br/>
/// <see cref="ClientType"/> = <see cref="Models.ClientType.WebSocket"/><br/>
/// <see cref="UseSSL"/> = false<br/>
/// <see cref="IsVerifiedBot"/> = false<br/>
/// </summary>
public sealed class ClientOptions
{
    /// <summary>
    /// The client type. Can be either a websocket or TCP connection.
    /// </summary>
    public ClientType ClientType { get; set; } = ClientType.WebSocket;

    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL { get; set; } = false;

    /// <summary>
    /// Indicates whether the bot is verified or not. If your bot is verified you can set this to true. Verified bots have higher rate limits.
    /// </summary>
    public bool IsVerifiedBot { get; set; } = false;
}
