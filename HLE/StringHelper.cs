using System;
using System.Collections.Generic;
using System.Linq;
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

    public static int[] IndicesOf(this string str, char c)
    {
        ReadOnlySpan<char> span = str;
        return span.IndicesOf(c);
    }

    public static int[] IndicesOf(this ReadOnlySpan<char> span, char c)
    {
        Span<int> indices = stackalloc int[span.Length];
        int count = 0;
        int idx = span.IndexOf(c);
        int totalIdx = idx;
        while (idx != -1)
        {
            indices[count++] = totalIdx;
            idx = span[++totalIdx..].IndexOf(c);
            totalIdx += idx;
        }

        return indices[..count].ToArray();
    }

    public static int[] IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s)
    {
        Span<int> indices = stackalloc int[span.Length];
        int count = 0;
        int idx = span.IndexOf(s);
        int totalIdx = idx;
        while (idx != -1)
        {
            indices[count++] = totalIdx;
            totalIdx += s.Length;
            idx = span[totalIdx..].IndexOf(s);
            totalIdx += idx;
        }

        return indices[..count].ToArray();
    }

    public static Range[] GetRangesOfSplit(this ReadOnlySpan<char> span, char separator = ' ')
    {
        int[] indices = span.IndicesOf(separator);
        if (indices.Length == 0)
        {
            return new[]
            {
                ..
            };
        }

        Span<Range> ranges = stackalloc Range[indices.Length + 1];
        int start = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            int idx = indices[i];
            ranges[i] = start..idx;
            start = idx + 1;
        }

        ranges[^1] = (indices[^1] + 1)..;
        return ranges.ToArray();
    }

    public static Range[] GetRangesOfSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator)
    {
        int[] indices = span.IndicesOf(separator);
        if (indices.Length == 0)
        {
            return new[]
            {
                ..
            };
        }

        Span<Range> ranges = stackalloc Range[indices.Length + 1];
        int start = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            int idx = indices[i];
            ranges[i] = start..idx;
            start = idx + separator.Length;
        }

        ranges[^1] = (indices[^1] + separator.Length)..;
        return ranges.ToArray();
    }
}
