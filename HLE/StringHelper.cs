using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    public static string Whitespace { get; } = " ";

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

    public static string[] Part(this string str, int charCount)
    {
        ReadOnlySpan<char> span = str;
        if (span.Length <= charCount)
        {
            return new[]
            {
                str
            };
        }

        Span<string> result = new string[span.Length / charCount + 1];
        int resultLength = 0;
        while (span.Length > charCount)
        {
            result[resultLength++] = new(span[..charCount]);
            span = span[charCount..];
        }

        result[resultLength++] = new(span);
        return result[..resultLength].ToArray();
    }

    public static string[] Part(this string str, int charCount, char separator)
    {
        ReadOnlySpan<char> span = str;
        if (span.Length <= charCount)
        {
            return new[]
            {
                str
            };
        }

        Span<Range> ranges = stackalloc Range[span.Length];
        int rangesLength = GetRangesOfSplit(span, separator, ranges);
        ranges = ranges[..rangesLength];

        Span<string> result = new string[ranges.Length];
        Span<char> buffer = stackalloc char[charCount];
        int resultLength = 0;
        int bufferLength = 0;
        for (int i = 0; i < ranges.Length; i++)
        {
            ReadOnlySpan<char> part = span[ranges[i]];
            if (part.Length >= charCount) // part doesn't fit into buffer, even if buffer is empty
            {
                if (bufferLength > 0) // if buffer isn't empty, write buffer into result
                {
                    result[resultLength++] = new(buffer[..bufferLength]);
                    bufferLength = 0;
                }

                result[resultLength++] = new(part);
            }
            else // part fits into buffer
            {
                switch (bufferLength)
                {
                    case > 0 when bufferLength + part.Length + 1 > charCount: // buffer is not empty and part doesn't fit in buffer
                        result[resultLength++] = new(buffer[..bufferLength]);
                        part.CopyTo(buffer);
                        bufferLength = part.Length;
                        break;
                    case > 0 when bufferLength + part.Length + 1 <= charCount: // buffer is not empty and part fits into buffer
                        buffer[bufferLength++] = separator;
                        part.CopyTo(buffer[bufferLength..]);
                        bufferLength += part.Length;
                        break;
                    case 0: // buffer is empty and part fits into buffer
                        part.CopyTo(buffer);
                        bufferLength = part.Length;
                        break;
                }
            }
        }

        if (bufferLength > 0) // if buffer isn't empty in the end, write buffer to result
        {
            result[resultLength++] = new(buffer[..bufferLength]);
        }

        return result[..resultLength].ToArray();
    }

    /// <summary>
    /// Trims all spaces in the beginning, end and middle of the <see cref="string"/> <paramref name="str"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be trimmed.</param>
    /// <returns>A trimmed <see cref="string"/>.</returns>
    public static string TrimAll(this string str)
    {
        return _multipleSpacesPattern.Replace(str.Trim(), Whitespace);
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
        return IndicesOf(span, c);
    }

    public static int[] IndicesOf(this ReadOnlySpan<char> span, char c)
    {
        Span<int> indices = stackalloc int[span.Length];
        int length = IndicesOf(span, c, indices);
        return indices[..length].ToArray();
    }

    internal static int IndicesOf(this ReadOnlySpan<char> span, char c, Span<int> indices)
    {
        int indicesLength = 0;
        int idx = span.IndexOf(c);
        int totalIdx = idx;
        while (idx != -1)
        {
            indices[indicesLength++] = totalIdx;
            idx = span[++totalIdx..].IndexOf(c);
            totalIdx += idx;
        }

        return indicesLength;
    }

    public static int[] IndicesOf(this string str, ReadOnlySpan<char> s)
    {
        ReadOnlySpan<char> span = str;
        return IndicesOf(span, s);
    }

    public static int[] IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s)
    {
        Span<int> indices = stackalloc int[span.Length];
        int length = IndicesOf(span, s, indices);
        return indices[..length].ToArray();
    }

    internal static int IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s, Span<int> indices)
    {
        int indicesLength = 0;
        int idx = span.IndexOf(s);
        int totalIdx = idx;
        while (idx != -1)
        {
            indices[indicesLength++] = totalIdx;
            totalIdx += s.Length;
            idx = span[totalIdx..].IndexOf(s);
            totalIdx += idx;
        }

        return indicesLength;
    }

    public static Range[] GetRangesOfSplit(this string str, char separator = ' ')
    {
        ReadOnlySpan<char> span = str;
        return GetRangesOfSplit(span, separator);
    }

    public static Range[] GetRangesOfSplit(this ReadOnlySpan<char> span, char separator = ' ')
    {
        Span<int> indices = stackalloc int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        Span<Range> ranges = stackalloc Range[indicesLength + 1];
        int rangesLength = GetRangesOfSplit(span, separator, ranges, indices);
        return ranges[..rangesLength].ToArray();
    }

    internal static int GetRangesOfSplit(this ReadOnlySpan<char> span, char separator, Span<Range> ranges)
    {
        Span<int> indices = stackalloc int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        indices = indices[..indicesLength];
        return GetRangesOfSplit(span, separator, ranges, indices);
    }

    internal static int GetRangesOfSplit(this ReadOnlySpan<char> span, char separator, Span<Range> ranges, Span<int> indices)
    {
        if (ranges.Length == 1)
        {
            ranges[0] = ..;
            return 1;
        }

        int start = 0;
        int length = 0;
        while (length < indices.Length)
        {
            int idx = indices[length];
            ranges[length++] = start..idx;
            start = idx + 1;
        }

        ranges[length++] = (indices[^1] + 1)..;
        return length;
    }

    public static Range[] GetRangesOfSplit(this string str, ReadOnlySpan<char> separator)
    {
        ReadOnlySpan<char> span = str;
        return GetRangesOfSplit(span, separator);
    }

    public static Range[] GetRangesOfSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator)
    {
        Span<Range> ranges = stackalloc Range[span.Length];
        int rangesLength = GetRangesOfSplit(span, separator, ranges);
        return ranges[..rangesLength].ToArray();
    }

    internal static int GetRangesOfSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator, Span<Range> ranges)
    {
        Span<int> indices = stackalloc int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        indices = indices[..indicesLength];
        ranges = ranges[..(indicesLength + 1)];
        return GetRangesOfSplit(span, separator, ranges, indices);
    }

    internal static int GetRangesOfSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator, Span<Range> ranges, Span<int> indices)
    {
        if (ranges.Length == 1)
        {
            ranges[0] = ..;
            return 1;
        }

        int start = 0;
        int length = 0;
        while (length < indices.Length)
        {
            int idx = indices[length];
            ranges[length++] = start..idx;
            start = idx + separator.Length;
        }

        ranges[length++] = (indices[^1] + separator.Length)..;
        return length;
    }

    public static Span<char> AsMutableSpan(this string? str)
    {
        ReadOnlySpan<char> span = str;
        return AsMutableSpan(span);
    }

    public static unsafe Span<char> AsMutableSpan(this ReadOnlySpan<char> span)
    {
        if (span.Length == 0)
        {
            return Span<char>.Empty;
        }

        ref char firstChar = ref MemoryMarshal.GetReference(span);
        char* pointer = (char*)Unsafe.AsPointer(ref firstChar);
        return new(pointer, span.Length);
    }

    public static void ToLower(string? str)
    {
        ToLower((ReadOnlySpan<char>)str);
    }

    public static void ToLower(ReadOnlySpan<char> span)
    {
        Span<char> mutSpan = span.AsMutableSpan();
        int spanLength = span.Length;
        for (int i = 0; i < spanLength; i++)
        {
            if (!char.IsLower(mutSpan[i]))
            {
                mutSpan[i] = char.ToLower(mutSpan[i]);
            }
        }
    }

    public static void ToLower(string? str, CultureInfo cultureInfo)
    {
        ToLower((ReadOnlySpan<char>)str, cultureInfo);
    }

    public static void ToLower(ReadOnlySpan<char> span, CultureInfo cultureInfo)
    {
        Span<char> mutSpan = span.AsMutableSpan();
        int spanLength = span.Length;
        for (int i = 0; i < spanLength; i++)
        {
            ref char c = ref mutSpan[i];
            if (!char.IsLower(c))
            {
                c = char.ToLower(c, cultureInfo);
            }
        }
    }

    public static void ToUpper(string? str)
    {
        ToUpper((ReadOnlySpan<char>)str);
    }

    public static void ToUpper(ReadOnlySpan<char> span)
    {
        Span<char> mutSpan = span.AsMutableSpan();
        int spanLength = span.Length;
        for (int i = 0; i < spanLength; i++)
        {
            if (!char.IsUpper(mutSpan[i]))
            {
                mutSpan[i] = char.ToUpper(mutSpan[i]);
            }
        }
    }

    public static void ToUpper(string? str, CultureInfo cultureInfo)
    {
        ToUpper((ReadOnlySpan<char>)str, cultureInfo);
    }

    public static void ToUpper(ReadOnlySpan<char> span, CultureInfo cultureInfo)
    {
        Span<char> mutSpan = span.AsMutableSpan();
        int spanLength = span.Length;
        for (int i = 0; i < spanLength; i++)
        {
            if (!char.IsUpper(mutSpan[i]))
            {
                mutSpan[i] = char.ToUpper(mutSpan[i], cultureInfo);
            }
        }
    }

    public static int CharCount(this string str, char c)
    {
        return CharCount((ReadOnlySpan<char>)str, c);
    }

    public static int CharCount(this ReadOnlySpan<char> span, char c)
    {
        int spanLength = span.Length;
        int charCount = 0;
        for (int i = 0; i < spanLength; i++)
        {
            if (span[i] == c)
            {
                charCount++;
            }
        }

        return charCount;
    }
}
