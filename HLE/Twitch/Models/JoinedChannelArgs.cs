using System;
using System.Text;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user joined a channel.
/// </summary>
public readonly struct JoinedChannelArgs
{
    /// <summary>
    /// The username of the user that joined the channel. All lower case.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The channel the user joined. All lower case, without '#'.
    /// </summary>
    public string Channel { get; }

    private const byte _exclamationMark = (byte)'!';

    /// <summary>
    /// The default constructor of <see cref="JoinedChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="indicesOfWhitespaces">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public JoinedChannelArgs(ReadOnlySpan<byte> ircMessage, Span<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        int idxExcl = firstWord.IndexOf(_exclamationMark);
        Username = Encoding.UTF8.GetString(firstWord[1..idxExcl]);
        Channel = Encoding.UTF8.GetString(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
    }
}
