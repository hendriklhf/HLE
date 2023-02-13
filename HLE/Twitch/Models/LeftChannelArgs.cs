using System;
using System.Text;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user left a channel.
/// </summary>
public readonly struct LeftChannelArgs
{
    /// <summary>
    /// The username of the user that left the channel. All lower case.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The channel the user left. All lower case, without '#'.
    /// </summary>
    public string Channel { get; }

    private const byte _exclamationMark = (byte)'!';

    /// <summary>
    /// The default constructor of <see cref="LeftChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="indicesOfWhitespaces">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public LeftChannelArgs(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        int indexOfExclamationMark = firstWord.IndexOf(_exclamationMark);
        Username = Encoding.UTF8.GetString(firstWord[1..indexOfExclamationMark]);
        Channel = Encoding.UTF8.GetString(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
    }
}
