using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Text;

namespace HLE.Twitch.Tmi;

internal static partial class ChannelFormatter
{
    [GeneratedRegex(@"^#?[a-z\d]\w{2,24}$", RegexOptions.Compiled | RegexOptions.IgnoreCase, 250)]
    private static partial Regex ChannelPattern { get; }

    public const char ChannelPrefix = '#';
    public const int MinimumChannelNameLength = 3;
    public const int MaximumChannelNameLength = 25;
    public const int MinimumPrefixedChannelNameLength = 4;
    public const int MaximumPrefixedChannelNameLength = 26;

    [Pure]
    [SkipLocalsInit]
    public static string Format(ReadOnlySpan<char> channel, bool prefixWithHashtag)
    {
        Span<char> result = stackalloc char[MaximumPrefixedChannelNameLength];
        int length = FormatChannel(channel, prefixWithHashtag, result);
        return StringPool.Shared.GetOrAdd(result[..length]);
    }

    private static int FormatChannel(ReadOnlySpan<char> channel, bool prefixWithHashtag, Span<char> result)
    {
        if (!ChannelPattern.IsMatch(channel))
        {
            ThrowInvalidChannelFormat(channel);
        }

        if (prefixWithHashtag)
        {
            if (channel[0] == ChannelPrefix)
            {
                channel.ToLowerInvariant(result);
                return channel.Length;
            }

            result[0] = ChannelPrefix;
            channel.ToLowerInvariant(result[1..]);
            return channel.Length + 1;
        }

        if (channel[0] == ChannelPrefix)
        {
            channel[1..].ToLowerInvariant(result);
            return channel.Length - 1;
        }

        channel.ToLowerInvariant(result);
        return channel.Length;
    }

    [DoesNotReturn]
    private static void ThrowInvalidChannelFormat(ReadOnlySpan<char> channel)
        => throw new FormatException($"The channel name (\"{channel}\") is in an invalid format. Expected: {ChannelPattern}");
}
