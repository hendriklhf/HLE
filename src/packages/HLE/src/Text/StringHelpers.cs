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

namespace HLE.Text;

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
    public static string TrimAll(string str) => TrimAll(str.AsSpan(), true);

    [Pure]
    public static string TrimAll(ref PooledInterpolatedStringHandler str)
    {
        string result = TrimAll(str.Text, false);
        str.Dispose();
        return result;
    }

    [Pure]
    [SkipLocalsInit]
    public static string TrimAll(ReadOnlySpan<char> str)
        => TrimAll(str, false);

    private static string TrimAll(ReadOnlySpan<char> str, [ConstantExpected] bool wasString)
    {
        if (str.Length == 0)
        {
            return string.Empty;
        }

        int resultLength;
        if (!MemoryHelpers.UseStackalloc<char>(str.Length))
        {
            char[] rentedBuffer = Memory.ArrayPool<char>.Shared.Rent(str.Length);
            resultLength = TrimAll(str, rentedBuffer.AsSpan());
            string result = resultLength == 0 && wasString ? StringMarshal.AsString(str) : new(rentedBuffer.AsSpanUnsafe(..resultLength));
            Memory.ArrayPool<char>.Shared.Return(rentedBuffer);
            return result;
        }

        Span<char> buffer = stackalloc char[str.Length];
        resultLength = TrimAll(str, buffer);
        return resultLength == 0 && wasString ? StringMarshal.AsString(str) : new(buffer[..resultLength]);
    }

    public static int TrimAll(string str, Span<char> result) => TrimAll(str.AsSpan(), result);

    public static int TrimAll(ref PooledInterpolatedStringHandler str, Span<char> result)
    {
        int charCount = TrimAll(str.Text, result);
        str.Dispose();
        return charCount;
    }

    public static int TrimAll(ReadOnlySpan<char> input, Span<char> result)
    {
        if (input.Length == 0)
        {
            return 0;
        }

        // TODO: this could be optimized
        if (input[0] != ' ' && input[^1] != ' ' && input.IndexOf("  ") < 0)
        {
            // No whitespace at the start or end, and no double spaces in between.
            return 0;
        }

        int indexOfAnyNonWhitespace = input.IndexOfAnyExcept(' ');
        if (indexOfAnyNonWhitespace < 0)
        {
            return 0;
        }

        input = input.SliceUnsafe(indexOfAnyNonWhitespace);
        input.CopyTo(result);
        result = result.SliceUnsafe(0, input.Length);

        int resultLength = result.Length;
        int indexOfWhitespace = result.IndexOf(' ');
        while (indexOfWhitespace >= 0)
        {
            result = result.SliceUnsafe(indexOfWhitespace);
            int whiteSpaceLength = result.IndexOfAnyExcept(' ');
            if (whiteSpaceLength < 0)
            {
                resultLength -= result.Length;
                break;
            }

            if (whiteSpaceLength == 1)
            {
                result = result.SliceUnsafe(1);
                indexOfWhitespace = result.IndexOf(' ');
                continue;
            }

            Span<char> source = result.SliceUnsafe(whiteSpaceLength..);
            Span<char> destination = result.SliceUnsafe(1..);
            source.CopyTo(destination);
            resultLength -= whiteSpaceLength - 1;
            result = result.SliceUnsafe(2..^(whiteSpaceLength - 1));

            indexOfWhitespace = result.IndexOf(' ');
        }

        return resultLength;
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
        if (span.Length == 0 || s.Length == 0)
        {
            return [];
        }

        int length;
        if (!MemoryHelpers.UseStackalloc<int>(span.Length))
        {
            int[] indicesBuffer = Memory.ArrayPool<int>.Shared.Rent(span.Length);
            length = IndicesOf(span, s, indicesBuffer.AsSpan());
            int[] result = indicesBuffer[..length];
            Memory.ArrayPool<int>.Shared.Return(indicesBuffer);
            return result;
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, s, indices);
        return indices.ToArray(..length);
    }

    public static int IndicesOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> s, Span<int> destination)
    {
        if (span.Length == 0 || s.Length == 0)
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
    public static string RegexEscape(string input) => RegexEscape(input, true);

    [Pure]
    public static string RegexEscape(ref PooledInterpolatedStringHandler input)
    {
        string result = RegexEscape(input.Text, false);
        input.Dispose();
        return result;
    }

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
            char[] rentedBuffer = Memory.ArrayPool<char>.Shared.Rent(maximumResultLength);
            resultLength = RegexEscape(input, rentedBuffer.AsSpan(), indexOfMetaChar);
            string result = inputIsString && input.Length == resultLength ? StringMarshal.AsString(input) : new(rentedBuffer.AsSpanUnsafe(..resultLength));
            Memory.ArrayPool<char>.Shared.Return(rentedBuffer);
            return result;
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
        Debug.Assert(input.Length != 0, $"If {nameof(indexOfMetaChar)} >= 0, the length of the input can't be 0.");

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
        if (strings.Length == 0)
        {
            return 0;
        }

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
        if (strings.Length == 0)
        {
            return 0;
        }

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

    public static int Join(ref PooledInterpolatedStringHandler separator, ReadOnlySpan<string> strings, Span<char> destination)
    {
        int charCount = Join(separator.Text, strings, destination);
        separator.Dispose();
        return charCount;
    }

    public static int Join(ReadOnlySpan<char> separator, ReadOnlySpan<string> strings, Span<char> destination)
    {
        if (strings.Length == 0)
        {
            return 0;
        }

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

    public static int Join(ref PooledInterpolatedStringHandler separator, ReadOnlySpan<ReadOnlyMemory<char>> strings, Span<char> destination)
    {
        int charCount = Join(separator.Text, strings, destination);
        separator.Dispose();
        return charCount;
    }

    public static int Join(ReadOnlySpan<char> separator, ReadOnlySpan<ReadOnlyMemory<char>> strings, Span<char> destination)
    {
        if (strings.Length == 0)
        {
            return 0;
        }

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
        if (chars.Length == 0)
        {
            return 0;
        }

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

    public static int Join(ref PooledInterpolatedStringHandler separator, ReadOnlySpan<char> chars, Span<char> destination)
    {
        int charCount = Join(separator.Text, chars, destination);
        separator.Dispose();
        return charCount;
    }

    public static int Join(ReadOnlySpan<char> separator, ReadOnlySpan<char> chars, Span<char> destination)
    {
        if (chars.Length == 0)
        {
            return 0;
        }

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
        if (strings.Length == 0)
        {
            return 0;
        }

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
        if (strings.Length == 0)
        {
            return 0;
        }

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
}
