using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
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
    /// Creates an invisible block in Twitch chat.
    /// </summary>
    public const char InvisibleBlockChar = '\u2800';

    /// <summary>
    /// Can be placed inside a username, which will not mention the user.
    /// </summary>
    public const string AntipingChar = "\uDB40\uDC00";

    public const string Whitespace = " ";

    [Pure]
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

        string[] result = new string[span.Length / charCount + 1];
        ref string firstResultItem = ref MemoryMarshal.GetArrayDataReference(result);
        int resultLength = 0;
        while (span.Length > charCount)
        {
            Unsafe.Add(ref firstResultItem, resultLength++) = new(span[..charCount]);
            span = span[charCount..];
        }

        Unsafe.Add(ref firstResultItem, resultLength++) = new(span);
        return resultLength == result.Length ? result : result[..resultLength];
    }

    [Pure]
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

        string[] result = new string[ranges.Length];
        ref string firstResultItem = ref MemoryMarshal.GetArrayDataReference(result);
        Span<char> buffer = stackalloc char[charCount];
        int resultLength = 0;
        int bufferLength = 0;
        for (int i = 0; i < rangesLength; i++)
        {
            ReadOnlySpan<char> part = span[ranges[i]];
            if (part.Length >= charCount) // part doesn't fit into buffer, even if buffer is empty
            {
                if (bufferLength > 0) // buffer isn't empty, write buffer into result
                {
                    Unsafe.Add(ref firstResultItem, resultLength++) = new(buffer[..bufferLength]);
                    bufferLength = 0;
                }

                Unsafe.Add(ref firstResultItem, resultLength++) = new(part);
            }
            else // part fits into buffer
            {
                switch (bufferLength)
                {
                    case > 0 when bufferLength + part.Length + 1 > charCount: // buffer is not empty and part doesn't fit in buffer
                        Unsafe.Add(ref firstResultItem, resultLength++) = new(buffer[..bufferLength]);
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
            Unsafe.Add(ref firstResultItem, resultLength++) = new(buffer[..bufferLength]);
        }

        return resultLength == result.Length ? result : result[..resultLength];
    }

    /// <summary>
    /// Trims all spaces in the beginning, end and middle of the <see cref="string"/> <paramref name="str"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be trimmed.</param>
    /// <returns>A trimmed <see cref="string"/>.</returns>
    [Pure]
    public static string TrimAll(this string str)
    {
        return _multipleSpacesPattern.Replace(str.Trim(), Whitespace);
    }

    [Pure]
    public static int[] IndicesOf(this string? str, char c)
    {
        return IndicesOf((ReadOnlySpan<char>)str, c);
    }

    public static int IndicesOf(this string? str, char c, Span<int> indices)
    {
        return IndicesOf((ReadOnlySpan<char>)str, c, indices);
    }

    [Pure]
    public static int[] IndicesOf(this ReadOnlySpan<char> span, char c)
    {
        Span<int> indices = stackalloc int[span.Length];
        int length = IndicesOf(span, c, indices);
        return indices[..length].ToArray();
    }

    public static int IndicesOf(this ReadOnlySpan<char> span, char c, Span<int> indices)
    {
        if (span.Length == 0)
        {
            return 0;
        }

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

    [Pure]
    public static int[] IndicesOf(this string? str, ReadOnlySpan<char> s)
    {
        return IndicesOf((ReadOnlySpan<char>)str, s);
    }

    public static int IndicesOf(this string? str, ReadOnlySpan<char> s, Span<int> indices)
    {
        return IndicesOf((ReadOnlySpan<char>)str, s, indices);
    }

    [Pure]
    public static int[] IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s)
    {
        Span<int> indices = stackalloc int[span.Length];
        int length = IndicesOf(span, s, indices);
        return indices[..length].ToArray();
    }

    public static int IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s, Span<int> indices)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        int indicesLength = 0;
        int idx = span.IndexOf(s);
        int totalIdx = idx;
        while (idx != -1)
        {
            indices[indicesLength++] = totalIdx;
            totalIdx += s.Length;
            idx = span[totalIdx..].IndexOf(s, StringComparison.Ordinal);
            totalIdx += idx;
        }

        return indicesLength;
    }

    [Pure]
    public static Range[] GetRangesOfSplit(this string? str, char separator = ' ')
    {
        return GetRangesOfSplit((ReadOnlySpan<char>)str, separator);
    }

    public static int GetRangesOfSplit(this string? str, char separator, Span<Range> ranges)
    {
        return GetRangesOfSplit((ReadOnlySpan<char>)str, separator, ranges);
    }

    [Pure]
    public static Range[] GetRangesOfSplit(this ReadOnlySpan<char> span, char separator = ' ')
    {
        if (span.Length == 0)
        {
            return Array.Empty<Range>();
        }

        Span<int> indices = stackalloc int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        Span<Range> ranges = stackalloc Range[indicesLength + 1];
        int rangesLength = GetRangesOfSplit(ranges, indices[..indicesLength]);
        return ranges[..rangesLength].ToArray();
    }

    public static int GetRangesOfSplit(this ReadOnlySpan<char> span, char separator, Span<Range> ranges)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        Span<int> indices = stackalloc int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        indices = indices[..indicesLength];
        return GetRangesOfSplit(ranges, indices);
    }

    public static int GetRangesOfSplit(Span<Range> ranges, ReadOnlySpan<int> indices)
    {
        switch (ranges.Length)
        {
            case 0:
                return 0;
            case > 0 when indices.Length == 0:
                ranges[0] = ..;
                return 1;
        }

        int start = 0;
        int rangesLength = 0;
        while (rangesLength < indices.Length)
        {
            int end = indices[rangesLength];
            ranges[rangesLength++] = start..end;
            start = end + 1;
        }

        ranges[rangesLength++] = (indices[^1] + 1)..;
        return rangesLength;
    }

    [Pure]
    public static Range[] GetRangesOfSplit(this string? str, ReadOnlySpan<char> separator)
    {
        return GetRangesOfSplit((ReadOnlySpan<char>)str, separator);
    }

    public static int GetRangesOfSplit(this string? str, ReadOnlySpan<char> separator, Span<Range> ranges)
    {
        return GetRangesOfSplit((ReadOnlySpan<char>)str, separator, ranges);
    }

    [Pure]
    public static Range[] GetRangesOfSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator)
    {
        if (span.Length == 0)
        {
            return Array.Empty<Range>();
        }

        Span<Range> ranges = stackalloc Range[span.Length];
        int rangesLength = GetRangesOfSplit(span, separator, ranges);
        return ranges[..rangesLength].ToArray();
    }

    public static int GetRangesOfSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator, Span<Range> ranges)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        Span<int> indices = stackalloc int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        indices = indices[..indicesLength];
        ranges = ranges[..(indicesLength + 1)];
        return GetRangesOfSplit(separator.Length, ranges, indices);
    }

    internal static int GetRangesOfSplit(int separatorLength, Span<Range> ranges, ReadOnlySpan<int> indices)
    {
        switch (ranges.Length)
        {
            case 0:
                return 0;
            case > 0 when indices.Length == 0:
                ranges[0] = ..;
                return 1;
        }

        int start = 0;
        int rangesLength = 0;
        while (rangesLength < indices.Length)
        {
            int end = indices[rangesLength];
            ranges[rangesLength++] = start..end;
            start = end + separatorLength;
        }

        ranges[rangesLength++] = (indices[^1] + separatorLength)..;
        return rangesLength;
    }

    /// <summary>
    /// With this method you would be able to mutate a <see cref="string"/>. ⚠️ Only use this if you completely know what you are doing and how strings work in C#. ⚠️
    /// </summary>
    /// <param name="str">The <see cref="string"/> that you will be able to mutate.</param>
    /// <returns>A <see cref="Span{Char}"/> representation of the passed-in <see cref="string"/>.</returns>
    [Pure]
    public static Span<char> AsMutableSpan(this string? str)
    {
        return AsMutableSpan((ReadOnlySpan<char>)str);
    }

    [Pure]
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

    public static unsafe void ToLower(ReadOnlySpan<char> span)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        char* chars = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        for (int i = 0; i < spanLength; i++)
        {
            char c = chars[i];
            if (!char.IsLower(c))
            {
                chars[i] = char.ToLower(c);
            }
        }
    }

    public static void ToLower(string? str, CultureInfo cultureInfo)
    {
        ToLower((ReadOnlySpan<char>)str, cultureInfo);
    }

    public static unsafe void ToLower(ReadOnlySpan<char> span, CultureInfo cultureInfo)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        char* chars = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        for (int i = 0; i < spanLength; i++)
        {
            char c = chars[i];
            if (!char.IsLower(c))
            {
                chars[i] = char.ToLower(c, cultureInfo);
            }
        }
    }

    public static void ToUpper(string? str)
    {
        ToUpper((ReadOnlySpan<char>)str);
    }

    public static unsafe void ToUpper(ReadOnlySpan<char> span)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        char* chars = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        for (int i = 0; i < spanLength; i++)
        {
            char c = chars[i];
            if (!char.IsUpper(c))
            {
                chars[i] = char.ToUpper(c);
            }
        }
    }

    public static void ToUpper(string? str, CultureInfo cultureInfo)
    {
        ToUpper((ReadOnlySpan<char>)str, cultureInfo);
    }

    public static unsafe void ToUpper(ReadOnlySpan<char> span, CultureInfo cultureInfo)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        char* chars = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        for (int i = 0; i < spanLength; i++)
        {
            char c = chars[i];
            if (!char.IsUpper(c))
            {
                chars[i] = char.ToUpper(c, cultureInfo);
            }
        }
    }

    [Pure]
    public static int CharCount(this string? str, char c)
    {
        return CharCount((ReadOnlySpan<char>)str, c);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int CharCount(this ReadOnlySpan<char> span, char c)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return 0;
        }

        int result = 0;
        int vector256Count = Vector256<ushort>.Count;
        if (Vector256.IsHardwareAccelerated && spanLength > vector256Count)
        {
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<char, ushort>(span);
            Vector256<ushort> equalsVector = Vector256.Create((ushort)c);
            Vector256<ushort> oneVector = Vector256.Create((ushort)1);
            while (shortSpan.Length > vector256Count)
            {
                Vector256<ushort> vector = Vector256.Create(shortSpan[..vector256Count]);
                shortSpan = shortSpan[vector256Count..];
                Vector256<ushort> equals = Vector256.Equals(vector, equalsVector);
                Vector256<ushort> and = Vector256.BitwiseAnd(equals, Vector256.Create((ushort)1));
                result += Vector256.Dot(and, oneVector);
            }

            for (int i = 0; i < shortSpan.Length; i++)
            {
                bool equals = shortSpan[i] == '.';
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        int vector128Count = Vector128<ushort>.Count;
        if (Vector128.IsHardwareAccelerated && spanLength > vector128Count)
        {
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<char, ushort>(span);
            Vector128<ushort> equalsVector = Vector128.Create((ushort)c);
            Vector128<ushort> oneVector = Vector128.Create((ushort)1);
            while (shortSpan.Length > vector128Count)
            {
                Vector128<ushort> vector = Vector128.Create(shortSpan[..vector128Count]);
                shortSpan = shortSpan[vector128Count..];
                Vector128<ushort> equals = Vector128.Equals(vector, equalsVector);
                Vector128<ushort> and = Vector128.BitwiseAnd(equals, Vector128.Create((ushort)1));
                result += Vector128.Dot(and, oneVector);
            }

            for (int i = 0; i < shortSpan.Length; i++)
            {
                bool equals = shortSpan[i] == '.';
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        int vector64Count = Vector64<ushort>.Count;
        if (Vector64.IsHardwareAccelerated && spanLength > vector64Count)
        {
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<char, ushort>(span);
            Vector64<ushort> equalsVector = Vector64.Create((ushort)c);
            Vector64<ushort> oneVector = Vector64.Create((ushort)1);
            while (shortSpan.Length > vector64Count)
            {
                Vector64<ushort> vector = Vector64.Create(shortSpan[..vector64Count]);
                shortSpan = shortSpan[vector64Count..];
                Vector64<ushort> equals = Vector64.Equals(vector, equalsVector);
                Vector64<ushort> and = Vector64.BitwiseAnd(equals, Vector64.Create((ushort)1));
                result += Vector64.Dot(and, oneVector);
            }

            for (int i = 0; i < shortSpan.Length; i++)
            {
                bool equals = shortSpan[i] == '.';
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        ref char firstChar = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            bool equals = Unsafe.Add(ref firstChar, i) == c;
            result += Unsafe.As<bool, byte>(ref equals);
        }

        return result;
    }

    public static void Replace(string? str, char oldChar, char newChar)
    {
        Replace((ReadOnlySpan<char>)str, oldChar, newChar);
    }

    public static unsafe void Replace(ReadOnlySpan<char> span, char oldChar, char newChar)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        char* chars = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        for (int i = 0; i < spanLength; i++)
        {
            if (chars[i] == oldChar)
            {
                chars[i] = newChar;
            }
        }
    }

    public static void Replace(string? str, ReadOnlySpan<char> oldString, ReadOnlySpan<char> newString)
    {
        Replace((ReadOnlySpan<char>)str, oldString, newString);
    }

    public static void Replace(ReadOnlySpan<char> span, ReadOnlySpan<char> oldString, ReadOnlySpan<char> newString)
    {
        if (span.Length == 0)
        {
            return;
        }

        if (newString.Length != oldString.Length || span.Length < oldString.Length || span.Length < newString.Length)
        {
            throw new InvalidOperationException($"{nameof(newString)} and {nameof(oldString)} have to have the same length and have to be of the same or a shorter length than {nameof(span)} in order to mutate the string.");
        }

        if (span.Length == newString.Length)
        {
            newString.CopyTo(span.AsMutableSpan());
            return;
        }

        int replacementIndexOffset = newString.Length;
        Span<char> mutSpan = span.AsMutableSpan();
        int lastIndex = mutSpan.Length + 1;
        for (int i = replacementIndexOffset; i < lastIndex; i++)
        {
            Span<char> spanToCompare = mutSpan[(i - replacementIndexOffset)..i];
            if (((ReadOnlySpan<char>)spanToCompare).Equals(oldString, StringComparison.Ordinal))
            {
                newString.CopyTo(spanToCompare);
            }
        }
    }

    public static void Replace(string? str, ReadOnlySpan<char> newString)
    {
        Replace((ReadOnlySpan<char>)str, newString);
    }

    public static void Replace(ReadOnlySpan<char> span, ReadOnlySpan<char> newString)
    {
        if (span.Length != newString.Length)
        {
            throw new InvalidOperationException($"{nameof(span)} and {nameof(newString)} have to have the same length");
        }

        newString.CopyTo(span.AsMutableSpan());
    }

    public static int RegexEscape(ReadOnlySpan<char> input, Span<char> escapedInput)
    {
        StringBuilder builder = escapedInput;
        int inputLength = input.Length;
        ref char firstChar = ref MemoryMarshal.GetReference(input);
        for (int i = 0; i < inputLength; i++)
        {
            char c = Unsafe.Add(ref firstChar, i);
            switch (c)
            {
                case '\\':
                case '*':
                case '+':
                case '?':
                case '|':
                case '{':
                case '[':
                case '(':
                case ')':
                case '^':
                case '$':
                case '.':
                    builder.Append('\\', c);
                    continue;
                case ' ':
                    builder.Append('\\', 's');
                    continue;
                default:
                    builder.Append(c);
                    break;
            }
        }

        return builder.Length;
    }
}
