using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Strings;

/// <summary>
/// A class to help with any kind of <see cref="string"/>.
/// </summary>
public static class StringHelpers
{
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

    public const string RegexMetaChars = "\t\n\f\r #$()*+.?[\\^{|";

    private static readonly SearchValues<char> s_regexMetaCharsSearchValues = SearchValues.Create(RegexMetaChars);

    [Pure]
    public static ReadOnlyMemory<char>[] Chunk(this string str, int charCount)
    {
        if (str.Length == 0)
        {
            return [];
        }

        ReadOnlyMemory<char> strAsMemory = str.AsMemory();
        if (strAsMemory.Length <= charCount)
        {
            return [str.AsMemory()];
        }

        ReadOnlyMemory<char>[] result = new ReadOnlyMemory<char>[strAsMemory.Length / charCount + 1];
        int resultLength = 0;
        while (strAsMemory.Length > charCount)
        {
            result[resultLength++] = strAsMemory[..charCount];
            strAsMemory = strAsMemory[charCount..];
        }

        result[resultLength++] = strAsMemory;
        return resultLength == result.Length ? result : result[..resultLength];
    }

    [Pure]
    public static ReadOnlyMemory<char>[] Chunk(this string str, int charCount, char separator)
    {
        if (str.Length == 0)
        {
            return [];
        }

        ReadOnlyMemory<char> inputSpan = str.AsMemory();
        if (inputSpan.Length <= charCount)
        {
            return [str.AsMemory()];
        }

        // TODO: remove allocation in case stackalloc can not be used
        Span<Range> ranges = MemoryHelpers.UseStackalloc<Range>(inputSpan.Length) ? stackalloc Range[inputSpan.Length] : new Range[inputSpan.Length];
        int rangesLength = inputSpan.Span.Split(ranges, separator);

        ReadOnlyMemory<char>[] result = new ReadOnlyMemory<char>[rangesLength];
        ref ReadOnlyMemory<char> resultReference = ref MemoryMarshal.GetArrayDataReference(result);
        using RentedArray<char> charBuffer = Memory.ArrayPool<char>.Shared.RentAsRentedArray(charCount);
        int resultLength = 0;
        int bufferLength = 0;
        for (int i = 0; i < rangesLength; i++)
        {
            ReadOnlyMemory<char> part = inputSpan[ranges[i]];
            if (part.Length >= charCount) // part doesn't fit into buffer, even if buffer is empty
            {
                if (bufferLength != 0) // buffer isn't empty, write buffer into result
                {
                    Unsafe.Add(ref resultReference, resultLength++) = charBuffer.AsMemory(..bufferLength);
                    bufferLength = 0;
                }

                Unsafe.Add(ref resultReference, resultLength++) = part;
            }
            else // part fits into buffer
            {
                switch (bufferLength)
                {
                    case not 0 when bufferLength + part.Length + 1 > charCount: // buffer is not empty and part doesn't fit in buffer
                        Unsafe.Add(ref resultReference, resultLength++) = charBuffer.AsMemory(..bufferLength);
                        part.CopyTo(charBuffer.AsMemory());
                        bufferLength = part.Length;
                        break;
                    case not 0 when bufferLength + part.Length + 1 <= charCount: // buffer is not empty and part fits into buffer
                        charBuffer[bufferLength++] = separator;
                        part.CopyTo(charBuffer.AsMemory(bufferLength..));
                        bufferLength += part.Length;
                        break;
                    case 0: // buffer is empty and part fits into buffer
                        part.CopyTo(charBuffer.AsMemory());
                        bufferLength = part.Length;
                        break;
                }
            }
        }

        if (bufferLength != 0) // if buffer isn't empty in the end, write buffer to result
        {
            Unsafe.Add(ref resultReference, resultLength++) = charBuffer.AsMemory(..bufferLength);
        }

        return resultLength == result.Length ? result : result[..resultLength];
    }

    [Pure]
    [SkipLocalsInit]
    public static string TrimAll(this string str)
    {
        if (str.Length == 0)
        {
            return string.Empty;
        }

        int resultLength;
        if (!MemoryHelpers.UseStackalloc<char>(str.Length))
        {
            using RentedArray<char> rentedBuffer = Memory.ArrayPool<char>.Shared.RentAsRentedArray(str.Length);
            resultLength = TrimAll(str, rentedBuffer.AsSpan());
            return new(rentedBuffer[..resultLength]);
        }

        Span<char> buffer = stackalloc char[str.Length];
        resultLength = TrimAll(str, buffer);
        return new(buffer[..resultLength]);
    }

    public static int TrimAll(this string str, Span<char> result) => TrimAll(str.AsSpan(), result);

    public static int TrimAll(this ReadOnlySpan<char> span, Span<char> result)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        int indexOfAnyNonWhitespace = span.IndexOfAnyExcept(' ');
        if (indexOfAnyNonWhitespace < 0)
        {
            return 0;
        }

        span = span.SliceUnsafe(indexOfAnyNonWhitespace);
        span.CopyTo(result);
        result = result.SliceUnsafe(0, span.Length);

        int indexOfWhitespaces = result.IndexOf("  ");
        while (indexOfWhitespaces >= 0)
        {
            int endOfWhitespaces = result.SliceUnsafe(indexOfWhitespaces).IndexOfAnyExcept(' ');
            if (endOfWhitespaces < 1)
            {
                result = result.SliceUnsafe(0, indexOfWhitespaces);
                break;
            }

            Span<char> source = result.SliceUnsafe(indexOfWhitespaces + endOfWhitespaces);
            Span<char> destination = result.SliceUnsafe(indexOfWhitespaces + 1);
            source.CopyTo(destination);

            result = result.SliceUnsafe(..^(endOfWhitespaces - 1));
            indexOfWhitespaces = result.IndexOf("  ");
        }

        if (result[^1] == ' ')
        {
            result = result[..^1];
        }

        return result.Length;
    }

    [Pure]
    public static int[] IndicesOf(this string? str, char c) => str.AsSpan().IndicesOf(c);

    public static int IndicesOf(this string? str, char c, Span<int> destination) => str.AsSpan().IndicesOf(c, destination);

    [Pure]
    public static int[] IndicesOf(this string? str, ReadOnlySpan<char> s) => IndicesOf((ReadOnlySpan<char>)str, s);

    public static int IndicesOf(this string? str, ReadOnlySpan<char> s, Span<int> destination) => IndicesOf((ReadOnlySpan<char>)str, s, destination);

    [Pure]
    [SkipLocalsInit]
    public static int[] IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s)
    {
        if (span.Length == 0)
        {
            return [];
        }

        int length;
        if (!MemoryHelpers.UseStackalloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = Memory.ArrayPool<int>.Shared.RentAsRentedArray(span.Length);
            length = IndicesOf(span, s, indicesBuffer.AsSpan());
            return indicesBuffer.ToArray(..length);
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, s, indices);
        return indices.ToArray(..length);
    }

    public static int IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s, Span<int> destination)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        int indicesLength = 0;
        int idx = span.IndexOf(s);
        int spanStartIndex = idx;
        while (idx >= 0)
        {
            destination[indicesLength++] = spanStartIndex;
            spanStartIndex += s.Length;
            idx = span[spanStartIndex..].IndexOf(s);
            spanStartIndex += idx;
        }

        return indicesLength;
    }

    [Pure]
    public static string RegexEscape(string? input) => input is null ? string.Empty : RegexEscape(input, true);

    [Pure]
    public static string RegexEscape(ReadOnlySpan<char> input) => RegexEscape(input, false);

    [Pure]
    [SkipLocalsInit]
    private static string RegexEscape(ReadOnlySpan<char> input, [ConstantExpected] bool inputIsString)
    {
        if (input.Length == 0)
        {
            return string.Empty;
        }

        int indexOfMetaChar = input.IndexOfAny(s_regexMetaCharsSearchValues);
        if (indexOfMetaChar < 0)
        {
            return inputIsString ? StringMarshal.AsString(input) : new(input);
        }

        int resultLength;
        int maximumResultLength = input.Length << 1;
        if (!MemoryHelpers.UseStackalloc<char>(maximumResultLength))
        {
            using RentedArray<char> rentedBuffer = Memory.ArrayPool<char>.Shared.RentAsRentedArray(maximumResultLength);
            resultLength = RegexEscape(input, rentedBuffer.AsSpan(), indexOfMetaChar);
            return inputIsString && input.Length == resultLength ? StringMarshal.AsString(input) : new(rentedBuffer[..resultLength]);
        }

        Span<char> buffer = stackalloc char[maximumResultLength];
        resultLength = RegexEscape(input, buffer, indexOfMetaChar);
        return inputIsString && input.Length == resultLength ? StringMarshal.AsString(input) : new(buffer[..resultLength]);
    }

    public static int RegexEscape(ReadOnlySpan<char> input, Span<char> destination)
    {
        int indexOfMetaChar = input.IndexOfAny(s_regexMetaCharsSearchValues);
        if (indexOfMetaChar >= 0)
        {
            return RegexEscape(input, destination, indexOfMetaChar);
        }

        input.CopyTo(destination);
        return input.Length;
    }

    private static int RegexEscape(ReadOnlySpan<char> input, Span<char> destination, int indexOfMetaChar)
    {
        Debug.Assert(indexOfMetaChar >= 0, "This method should only be called if the input contains a meta char.");

        if (input.Length == 0)
        {
            return 0;
        }

        SearchValues<char> regexMetaCharsSearchValues = s_regexMetaCharsSearchValues;
        int resultLength = 0;
        do
        {
            char metaChar = input[indexOfMetaChar];
            metaChar = metaChar switch
            {
                '\n' => 'n',
                '\r' => 'r',
                '\t' => 't',
                '\f' => 'f',
                _ => metaChar
            };

            ReadOnlySpan<char> start = input.SliceUnsafe(0, indexOfMetaChar);
            start.CopyTo(destination[resultLength..]);
            resultLength += start.Length;

            destination[resultLength++] = '\\';
            destination[resultLength++] = metaChar;

            input = input.SliceUnsafe(indexOfMetaChar + 1);
            indexOfMetaChar = input.IndexOfAny(regexMetaCharsSearchValues);
        }
        while (indexOfMetaChar >= 0);

        input.CopyTo(destination[resultLength..]);
        return resultLength + input.Length;
    }

    public static int Join(char separator, ReadOnlySpan<string> strings, Span<char> destination)
    {
        int stringsLengthMinus1 = strings.Length - 1;
        ref string stringsReference = ref MemoryMarshal.GetReference(strings);
        int resultLength = 0;
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            string str = Unsafe.Add(ref stringsReference, i);
            str.CopyTo(destination[resultLength..]);
            resultLength += str.Length;
            destination[resultLength++] = separator;
        }

        string lastString = Unsafe.Add(ref stringsReference, stringsLengthMinus1);
        lastString.CopyTo(destination[resultLength..]);
        return resultLength + lastString.Length;
    }

    public static int Join(char separator, ReadOnlySpan<ReadOnlyMemory<char>> strings, Span<char> destination)
    {
        int stringsLengthMinus1 = strings.Length - 1;
        ref ReadOnlyMemory<char> stringsReference = ref MemoryMarshal.GetReference(strings);
        int resultLength = 0;
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            ReadOnlyMemory<char> str = Unsafe.Add(ref stringsReference, i);
            str.Span.CopyTo(destination[resultLength..]);
            resultLength += str.Length;
            destination[resultLength++] = separator;
        }

        ReadOnlyMemory<char> lastString = Unsafe.Add(ref stringsReference, stringsLengthMinus1);
        lastString.Span.CopyTo(destination[resultLength..]);
        return resultLength + lastString.Length;
    }

    public static int Join(ReadOnlySpan<char> separator, ReadOnlySpan<string> strings, Span<char> destination)
    {
        int stringsLengthMinus1 = strings.Length - 1;
        ref string stringsReference = ref MemoryMarshal.GetReference(strings);
        int resultLength = 0;
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            string str = Unsafe.Add(ref stringsReference, i);
            str.CopyTo(destination[resultLength..]);
            resultLength += str.Length;
            separator.CopyTo(destination[resultLength..]);
            resultLength += separator.Length;
        }

        string lastString = Unsafe.Add(ref stringsReference, stringsLengthMinus1);
        lastString.CopyTo(destination[resultLength..]);
        return resultLength + lastString.Length;
    }

    public static int Join(ReadOnlySpan<char> separator, ReadOnlySpan<ReadOnlyMemory<char>> strings, Span<char> destination)
    {
        int stringsLengthMinus1 = strings.Length - 1;
        ref ReadOnlyMemory<char> stringsReference = ref MemoryMarshal.GetReference(strings);
        int resultLength = 0;
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            ReadOnlyMemory<char> str = Unsafe.Add(ref stringsReference, i);
            str.Span.CopyTo(destination[resultLength..]);
            resultLength += str.Length;
            separator.CopyTo(destination[resultLength..]);
            resultLength += separator.Length;
        }

        ReadOnlyMemory<char> lastString = Unsafe.Add(ref stringsReference, stringsLengthMinus1);
        lastString.Span.CopyTo(destination[resultLength..]);
        return resultLength + lastString.Length;
    }

    public static int Join(char separator, ReadOnlySpan<char> chars, Span<char> destination)
    {
        int charsLengthMinus1 = chars.Length - 1;
        ref char charsReference = ref MemoryMarshal.GetReference(chars);
        int resultLength = 0;
        for (int i = 0; i < charsLengthMinus1; i++)
        {
            destination[resultLength++] = Unsafe.Add(ref charsReference, i);
            destination[resultLength++] = separator;
        }

        destination[resultLength++] = Unsafe.Add(ref charsReference, charsLengthMinus1);
        return resultLength;
    }

    public static int Join(ReadOnlySpan<char> separator, ReadOnlySpan<char> chars, Span<char> destination)
    {
        int charsLengthMinus1 = chars.Length - 1;
        ref char charsReference = ref MemoryMarshal.GetReference(chars);
        int resultLength = 0;
        for (int i = 0; i < charsLengthMinus1; i++)
        {
            destination[resultLength++] = Unsafe.Add(ref charsReference, i);
            separator.CopyTo(destination[resultLength..]);
            resultLength += separator.Length;
        }

        destination[resultLength++] = Unsafe.Add(ref charsReference, charsLengthMinus1);
        return resultLength;
    }

    public static int Concat(ReadOnlySpan<string> strings, Span<char> destination)
    {
        int stringsLength = strings.Length;
        ref string stringsReference = ref MemoryMarshal.GetReference(strings);
        int resultLength = 0;
        for (int i = 0; i < stringsLength; i++)
        {
            string str = Unsafe.Add(ref stringsReference, i);
            str.CopyTo(destination[resultLength..]);
            resultLength += str.Length;
        }

        return resultLength;
    }

    public static int Concat(ReadOnlySpan<ReadOnlyMemory<char>> strings, Span<char> destination)
    {
        int stringsLength = strings.Length;
        ref ReadOnlyMemory<char> stringsReference = ref MemoryMarshal.GetReference(strings);
        int resultLength = 0;
        for (int i = 0; i < stringsLength; i++)
        {
            ReadOnlyMemory<char> str = Unsafe.Add(ref stringsReference, i);
            str.Span.CopyTo(destination[resultLength..]);
            resultLength += str.Length;
        }

        return resultLength;
    }

    /// <summary>
    /// Returns the UTF-16 bytes of a string by reinterpreting the chars the string consists of.<br/>
    /// Basically returns the same as <c>Encoding.Unicode.GetBytes(string)</c> without allocating.
    /// </summary>
    /// <param name="str">The string of which the bytes will be read from.</param>
    /// <returns>A span of UTF-16 bytes of the string.</returns>
    [Pure]
    public static ReadOnlySpan<byte> AsUtf16Bytes(this string? str)
    {
        if (str is null)
        {
            return [];
        }

        ref char reference = ref StringMarshal.GetReference(str);
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<char, byte>(ref reference), str.Length << 1);
    }
}
