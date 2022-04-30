using System;
using System.Text.RegularExpressions;

namespace HLE.Twitch;

internal static class Utils
{
    public static Regex EndingNumbersPattern { get; } = new(@"-?\d+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    public static Regex EndingWordPattern { get; } = new(@"\w+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
}
