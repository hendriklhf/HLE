using System;
using System.Text.RegularExpressions;

namespace HLE.Twitch;

internal static class Utils
{
    internal static Regex EndingNumbersPattern { get; } = new(@"-?\d+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    internal static Regex EndingWordPattern { get; } = new(@"\w+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
}
