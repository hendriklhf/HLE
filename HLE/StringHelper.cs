using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace HLE;

/// <summary>
/// A class to help with any kind of <see cref="string"/>.
/// </summary>
public static class StringHelper
{
    private static readonly Regex _multipleSpacesPattern = new(@"\s{2,}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Is invisible in Twitch chat.
    /// </summary>
    public const char InvisibleChar = '\uFFFD';

    /// <summary>
    /// Creates a invisible block in Twitch chat.
    /// </summary>
    public const char InvisibleBlockChar = '\u2800';

    /// <summary>
    /// Can be placed inside a username, which not mention the user.
    /// </summary>
    public const string AntipingChar = "\uDB40\uDC00";

    /// <summary>
    /// Removes the given <see cref="string"/> <paramref name="s"/> from the input <see cref="string"/> <paramref name="str"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> from with the given <see cref="string"/> <paramref name="s"/> will be removed.</param>
    /// <param name="s">The <see cref="string"/> that will be removed from the input <see cref="string"/> <paramref name="str"/>.</param>
    /// <returns>Returns the <see cref="string"/> <paramref name="str"/> with the <paramref name="s"/> removed.</returns>
    public static string Remove(this string str, string s)
    {
        return str.Replace(s, string.Empty);
    }

    public static string[] Split(this string str, int charCount, bool onlySplitOnWhitespace = false)
    {
        str = str.TrimAll();
        if (str.Length <= charCount)
        {
            return new[]
            {
                str
            };
        }

        if (!onlySplitOnWhitespace)
        {
            List<string> result = new();
            while (str.Length > charCount)
            {
                result.Add(str[..charCount]);
                str = str[charCount..];
            }

            result.Add(str);
            return result.ToArray();
        }

        string[] split = str.Split();
        List<List<string>> list = new();
        int listIdx = 0;
        int sum = 0;
        foreach (string s in split)
        {
            if (list.Count < listIdx + 1)
            {
                list.Add(new());
            }

            bool exceedsMaxCharCount = sum + list[listIdx].Count + s.Length > charCount;
            if (!exceedsMaxCharCount)
            {
                list[listIdx].Add(s);
                sum += s.Length;
            }
            else
            {
                if (sum == 0)
                {
                    list[listIdx].Add(s);
                }
                else
                {
                    list.Add(new());
                    list[++listIdx].Add(s);
                }

                listIdx++;
                sum = 0;
            }
        }

        return list.Select(l => string.Join(' ', l)).ToArray();
    }

    /// <summary>
    /// Trims all spaces in the beginning, end and middle of the <see cref="string"/> <paramref name="str"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be trimmed.</param>
    /// <returns>A trimmed <see cref="string"/>.</returns>
    public static string TrimAll(this string str)
    {
        return _multipleSpacesPattern.Replace(str.Trim(), " ");
    }

    public static StringBuilder Append(this StringBuilder builder, params string[] strings)
    {
        foreach (string s in strings)
        {
            builder.Append(s);
        }

        return builder;
    }

    public static string InsertKDots<T>(this T number, char kchar = '.') where T : INumber<T>
    {
        string? num = number.ToString();
        if (num is null)
        {
            throw new ArgumentNullException($"The conversion of {nameof(number)} to string returned null.");
        }

        if (num.Length < 4)
        {
            return num;
        }

        int dotCount = num.Length % 3 == 0 ? num.Length / 3 - 1 : num.Length / 3;
        int total = num.Length + dotCount;
        Span<char> span = stackalloc char[total];
        int start = num.Length % 3;
        if (start == 0)
        {
            start += 3;
        }

        int nextDot = start;
        for (int i = 0; i < total; i++)
        {
            if (i == nextDot)
            {
                span[i] = kchar;
                nextDot += 4;
            }
            else
            {
                span[i] = num[i - (nextDot - start >> 2)];
            }
        }

        return new(span);
    }

    public static int[] IndicesOf(this string str, char c)
    {
        Span<int> indices = stackalloc int[str.Length];
        int count = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == c)
            {
                indices[count++] = i;
            }
        }

        return indices[..count].ToArray();
    }

    public static int[] IndicesOf(this ReadOnlySpan<char> span, char c)
    {
        Span<int> indices = stackalloc int[span.Length];
        int count = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == c)
            {
                indices[count++] = i;
            }
        }

        return indices[..count].ToArray();
    }
}
