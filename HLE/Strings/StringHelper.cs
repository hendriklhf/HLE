using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text.RegularExpressions;
using HLE.Memory;

namespace HLE.Strings;

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

    [Pure]
    public static ReadOnlyMemory<char>[] Part(this string str, int charCount)
    {
        ReadOnlyMemory<char> span = str.AsMemory();
        if (span.Length <= charCount)
        {
            return new[]
            {
                str.AsMemory()
            };
        }

        ReadOnlyMemory<char>[] result = new ReadOnlyMemory<char>[span.Length / charCount + 1];
        ref ReadOnlyMemory<char> firstResultItem = ref MemoryMarshal.GetArrayDataReference(result);
        int resultLength = 0;
        while (span.Length > charCount)
        {
            Unsafe.Add(ref firstResultItem, resultLength++) = span[..charCount];
            span = span[charCount..];
        }

        Unsafe.Add(ref firstResultItem, resultLength++) = span;
        return resultLength == result.Length ? result : result[..resultLength];
    }

    [Pure]
    public static ReadOnlyMemory<char>[] Part(this string str, int charCount, char separator)
    {
        ReadOnlyMemory<char> span = str.AsMemory();
        if (span.Length <= charCount)
        {
            return new[]
            {
                str.AsMemory()
            };
        }

        Span<Range> ranges = MemoryHelper.UseStackAlloc<Range>(span.Length) ? stackalloc Range[span.Length] : new Range[span.Length];
        int rangesLength = GetRangesOfSplit(span.Span, separator, ranges);

        ReadOnlyMemory<char>[] result = new ReadOnlyMemory<char>[ranges.Length];
        ref ReadOnlyMemory<char> firstResultItem = ref MemoryMarshal.GetArrayDataReference(result);
        using RentedArray<char> buffer = new(charCount);
        int resultLength = 0;
        int bufferLength = 0;
        for (int i = 0; i < rangesLength; i++)
        {
            ReadOnlyMemory<char> part = span[ranges[i]];
            if (part.Length >= charCount) // part doesn't fit into buffer, even if buffer is empty
            {
                if (bufferLength > 0) // buffer isn't empty, write buffer into result
                {
                    Unsafe.Add(ref firstResultItem, resultLength++) = buffer.Memory[..bufferLength];
                    bufferLength = 0;
                }

                Unsafe.Add(ref firstResultItem, resultLength++) = part;
            }
            else // part fits into buffer
            {
                switch (bufferLength)
                {
                    case > 0 when bufferLength + part.Length + 1 > charCount: // buffer is not empty and part doesn't fit in buffer
                        Unsafe.Add(ref firstResultItem, resultLength++) = buffer.Memory[..bufferLength];
                        part.CopyTo(buffer);
                        bufferLength = part.Length;
                        break;
                    case > 0 when bufferLength + part.Length + 1 <= charCount: // buffer is not empty and part fits into buffer
                        buffer[bufferLength++] = separator;
                        part.CopyTo(buffer.Memory[bufferLength..]);
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
            Unsafe.Add(ref firstResultItem, resultLength++) = buffer.Memory[..bufferLength];
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
        return _multipleSpacesPattern.Replace(str.Trim(), " ");
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
        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
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
        int spanStartIndex = idx;
        while (idx != -1)
        {
            indices[indicesLength++] = spanStartIndex;
            idx = span[++spanStartIndex..].IndexOf(c);
            spanStartIndex += idx;
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
        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
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
        int spanStartIndex = idx;
        while (idx != -1)
        {
            indices[indicesLength++] = spanStartIndex;
            spanStartIndex += s.Length;
            idx = span[spanStartIndex..].IndexOf(s, StringComparison.Ordinal);
            spanStartIndex += idx;
        }

        return indicesLength;
    }

    public static int IndicesOf(this ReadOnlySpan<byte> span, byte b, Span<int> indices)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return 0;
        }

        int length = 0;
        ref byte firstIndex = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            bool equals = Unsafe.Add(ref firstIndex, i) == b;
            byte asByte = Unsafe.As<bool, byte>(ref equals);
            indices[length] = i;
            length += asByte;
        }

        return length;
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

        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        Span<Range> ranges = MemoryHelper.UseStackAlloc<Range>(indicesLength + 1) ? stackalloc Range[indicesLength + 1] : new Range[indicesLength + 1];
        int rangesLength = GetRangesOfSplit(ranges, indices[..indicesLength]);
        return ranges[..rangesLength].ToArray();
    }

    public static int GetRangesOfSplit(this ReadOnlySpan<char> span, char separator, Span<Range> ranges)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        return GetRangesOfSplit(ranges, indices[..indicesLength]);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int GetRangesOfSplit(Span<Range> ranges, ReadOnlySpan<int> indices, int separatorLength = 1)
    {
        switch (ranges.Length)
        {
            case 0:
                return 0;
            case > 0 when indices.Length == 0:
                ranges[0] = ..;
                return 1;
        }

        ref int rangesAsInt = ref Unsafe.As<Range, int>(ref MemoryMarshal.GetReference(ranges));

        int rangesAsIntLength = 0;
#if NET8_0_OR_GREATER
        int vector512Count = Vector512<int>.Count;
        if (Vector512.IsHardwareAccelerated && indices.Length >= vector512Count)
        {
            var separatorLengthVector = Vector512.Create(separatorLength);
            while (indices.Length > vector512Count)
            {
                ref int firstIndex = ref MemoryMarshal.GetReference(indices);
                var endIndices = Vector512.LoadUnsafe(ref firstIndex);
                var startIndices = Vector512.Add(endIndices, separatorLengthVector);

                var firstHalf = Vector512.Create(endIndices[0], startIndices[0], endIndices[1], startIndices[1], endIndices[2], startIndices[2], endIndices[3], startIndices[3], endIndices[4], startIndices[4],
                    endIndices[5], startIndices[5], endIndices[6], startIndices[6], endIndices[7], startIndices[7]);
                var secondHalf = Vector512.Create(endIndices[8], startIndices[8], endIndices[9], startIndices[9], endIndices[10], startIndices[10], endIndices[11], startIndices[11], endIndices[12], startIndices[12],
                    endIndices[13], startIndices[13], endIndices[14], startIndices[14], endIndices[15], startIndices[15]);
                firstHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + 1));
                secondHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + vector512Count + 1));

                indices = indices[vector512Count..];
                rangesAsIntLength += vector512Count << 1;
            }

            rangesAsIntLength++;
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index;
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index + separatorLength;
            }

            Unsafe.Add(ref rangesAsInt, 0) = 0;
            Unsafe.Add(ref rangesAsInt, rangesAsIntLength) = ~0;
            return (rangesAsIntLength >> 1) + 1;
        }
#endif

        int vector256Count = Vector256<int>.Count;
        if (Vector256.IsHardwareAccelerated && indices.Length >= vector256Count)
        {
            var separatorLengthVector = Vector256.Create(separatorLength);
            while (indices.Length > vector256Count)
            {
                ref int firstIndex = ref MemoryMarshal.GetReference(indices);
                var endIndices = Vector256.LoadUnsafe(ref firstIndex);
                var startIndices = Vector256.Add(endIndices, separatorLengthVector);

                var firstHalf = Vector256.Create(endIndices[0], startIndices[0], endIndices[1], startIndices[1], endIndices[2], startIndices[2], endIndices[3], startIndices[3]);
                var secondHalf = Vector256.Create(endIndices[4], startIndices[4], endIndices[5], startIndices[5], endIndices[6], startIndices[6], endIndices[7], startIndices[7]);
                firstHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + 1));
                secondHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + vector256Count + 1));

                indices = indices[vector256Count..];
                rangesAsIntLength += vector256Count << 1;
            }

            rangesAsIntLength++;
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index;
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index + separatorLength;
            }

            Unsafe.Add(ref rangesAsInt, 0) = 0;
            Unsafe.Add(ref rangesAsInt, rangesAsIntLength) = ~0;
            return (rangesAsIntLength >> 1) + 1;
        }

        int vector128Count = Vector128<int>.Count;
        if (Vector128.IsHardwareAccelerated && indices.Length >= vector128Count)
        {
            var separatorLengthVector = Vector128.Create(separatorLength);
            while (indices.Length > vector128Count)
            {
                ref int firstIndex = ref MemoryMarshal.GetReference(indices);
                var endIndices = Vector128.LoadUnsafe(ref firstIndex);
                var startIndices = Vector128.Add(endIndices, separatorLengthVector);

                var firstHalf = Vector128.Create(endIndices[0], startIndices[0], endIndices[1], startIndices[1]);
                var secondHalf = Vector128.Create(endIndices[2], startIndices[2], endIndices[3], startIndices[3]);
                firstHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + 1));
                secondHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + vector128Count + 1));

                indices = indices[vector128Count..];
                rangesAsIntLength += vector128Count << 1;
            }

            rangesAsIntLength++;
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index;
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index + separatorLength;
            }

            Unsafe.Add(ref rangesAsInt, 0) = 0;
            Unsafe.Add(ref rangesAsInt, rangesAsIntLength) = ~0;
            return (rangesAsIntLength >> 1) + 1;
        }

        int vector64Count = Vector64<int>.Count;
        if (Vector64.IsHardwareAccelerated && indices.Length >= vector64Count)
        {
            var separatorLengthVector = Vector64.Create(separatorLength);
            while (indices.Length > vector64Count)
            {
                ref int firstIndex = ref MemoryMarshal.GetReference(indices);
                var endIndices = Vector64.LoadUnsafe(ref firstIndex);
                var startIndices = Vector64.Add(endIndices, separatorLengthVector);

                var firstHalf = Vector64.Create(endIndices[0], startIndices[0]);
                var secondHalf = Vector64.Create(endIndices[1], startIndices[1]);
                firstHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + 1));
                secondHalf.StoreUnsafe(ref Unsafe.Add(ref rangesAsInt, rangesAsIntLength + vector64Count + 1));

                indices = indices[vector64Count..];
                rangesAsIntLength += vector64Count << 1;
            }

            rangesAsIntLength++;
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index;
                Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index + separatorLength;
            }

            Unsafe.Add(ref rangesAsInt, 0) = 0;
            Unsafe.Add(ref rangesAsInt, rangesAsIntLength) = ~0;
            return (rangesAsIntLength >> 1) + 1;
        }

        rangesAsIntLength++;
        for (int i = 0; i < indices.Length; i++)
        {
            int index = indices[i];
            Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index;
            Unsafe.Add(ref rangesAsInt, rangesAsIntLength++) = index + separatorLength;
        }

        Unsafe.Add(ref rangesAsInt, 0) = 0;
        Unsafe.Add(ref rangesAsInt, rangesAsIntLength) = ~0;
        return (rangesAsIntLength >> 1) + 1;
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

        Span<Range> ranges = MemoryHelper.UseStackAlloc<Range>(span.Length) ? stackalloc Range[span.Length] : new Range[span.Length];
        int rangesLength = GetRangesOfSplit(span, separator, ranges);
        return ranges[..rangesLength].ToArray();
    }

    public static int GetRangesOfSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator, Span<Range> ranges)
    {
        if (span.Length == 0)
        {
            return 0;
        }

        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        ranges = ranges[..(indicesLength + 1)];
        return GetRangesOfSplit(ranges, indices[..indicesLength], separator.Length);
    }

    /// <summary>
    /// Vectorized char count.
    /// </summary>
    /// <param name="str">The string in which the char will be counted.</param>
    /// <param name="charToCount">The char that will be counted.</param>
    /// <returns>The amount of the char <paramref name="charToCount"/> in the string <paramref name="str"/>.</returns>
    [Pure]
    public static int CharCount(this string? str, char charToCount)
    {
        return CharCount((ReadOnlySpan<char>)str, charToCount);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int CharCount(this ReadOnlySpan<char> span, char charToCount)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return 0;
        }

        int result = 0;
#if NET8_0_OR_GREATER
        int vector512Count = Vector512<ushort>.Count;
        if (Vector512.IsHardwareAccelerated && spanLength >= vector512Count)
        {
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<char, ushort>(span);
            Vector512<ushort> equalsVector = Vector512.Create((ushort)charToCount);
            Vector512<ushort> oneVector = Vector512.Create((ushort)1);
            while (shortSpan.Length >= vector512Count)
            {
                Vector512<ushort> vector = Vector512.Create(shortSpan[..vector512Count]);
                Vector512<ushort> equals = Vector512.Equals(vector, equalsVector);
                Vector512<ushort> and = Vector512.BitwiseAnd(equals, oneVector);
                result += Vector512.Sum(and);
                shortSpan = shortSpan[vector512Count..];
            }

            for (int i = 0; i < shortSpan.Length; i++)
            {
                bool equals = shortSpan[i] == charToCount;
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }
#endif

        int vector256Count = Vector256<ushort>.Count;
        if (Vector256.IsHardwareAccelerated && spanLength >= vector256Count)
        {
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<char, ushort>(span);
            Vector256<ushort> equalsVector = Vector256.Create((ushort)charToCount);
            Vector256<ushort> oneVector = Vector256.Create((ushort)1);
            while (shortSpan.Length >= vector256Count)
            {
                Vector256<ushort> vector = Vector256.Create(shortSpan[..vector256Count]);
                Vector256<ushort> equals = Vector256.Equals(vector, equalsVector);
                Vector256<ushort> and = Vector256.BitwiseAnd(equals, oneVector);
                result += Vector256.Sum(and);
                shortSpan = shortSpan[vector256Count..];
            }

            for (int i = 0; i < shortSpan.Length; i++)
            {
                bool equals = shortSpan[i] == charToCount;
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        int vector128Count = Vector128<ushort>.Count;
        if (Vector128.IsHardwareAccelerated && spanLength >= vector128Count)
        {
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<char, ushort>(span);
            Vector128<ushort> equalsVector = Vector128.Create((ushort)charToCount);
            Vector128<ushort> oneVector = Vector128.Create((ushort)1);
            while (shortSpan.Length >= vector128Count)
            {
                Vector128<ushort> vector = Vector128.Create(shortSpan[..vector128Count]);
                Vector128<ushort> equals = Vector128.Equals(vector, equalsVector);
                Vector128<ushort> and = Vector128.BitwiseAnd(equals, oneVector);
                result += Vector128.Sum(and);
                shortSpan = shortSpan[vector128Count..];
            }

            for (int i = 0; i < shortSpan.Length; i++)
            {
                bool equals = shortSpan[i] == charToCount;
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        int vector64Count = Vector64<ushort>.Count;
        if (Vector64.IsHardwareAccelerated && spanLength >= vector64Count)
        {
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<char, ushort>(span);
            Vector64<ushort> equalsVector = Vector64.Create((ushort)charToCount);
            Vector64<ushort> oneVector = Vector64.Create((ushort)1);
            while (shortSpan.Length >= vector64Count)
            {
                Vector64<ushort> vector = Vector64.Create(shortSpan[..vector64Count]);
                Vector64<ushort> equals = Vector64.Equals(vector, equalsVector);
                Vector64<ushort> and = Vector64.BitwiseAnd(equals, oneVector);
                result += Vector64.Sum(and);
                shortSpan = shortSpan[vector64Count..];
            }

            for (int i = 0; i < shortSpan.Length; i++)
            {
                bool equals = shortSpan[i] == charToCount;
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        ref char firstChar = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            bool equals = Unsafe.Add(ref firstChar, i) == charToCount;
            result += Unsafe.As<bool, byte>(ref equals);
        }

        return result;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int ByteCount(this ReadOnlySpan<byte> span, byte byteToCount)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return 0;
        }

        int result = 0;
#if NET8_0_OR_GREATER
        int vector512Count = Vector512<byte>.Count;
        if (Vector512.IsHardwareAccelerated && spanLength >= vector512Count)
        {
            Vector512<byte> equalsVector = Vector512.Create(byteToCount);
            Vector512<byte> oneVector = Vector512.Create((byte)1);
            while (span.Length >= vector512Count)
            {
                Vector512<byte> vector = Vector512.Create(span[..vector512Count]);
                span = span[vector512Count..];
                Vector512<byte> equals = Vector512.Equals(vector, equalsVector);
                Vector512<byte> and = Vector512.BitwiseAnd(equals, oneVector);
                result += Vector512.Sum(and);
            }

            for (int i = 0; i < span.Length; i++)
            {
                bool equals = span[i] == byteToCount;
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }
#endif

        int vector256Count = Vector256<byte>.Count;
        if (Vector256.IsHardwareAccelerated && spanLength >= vector256Count)
        {
            Vector256<byte> equalsVector = Vector256.Create(byteToCount);
            Vector256<byte> oneVector = Vector256.Create((byte)1);
            while (span.Length >= vector256Count)
            {
                Vector256<byte> vector = Vector256.Create(span[..vector256Count]);
                span = span[vector256Count..];
                Vector256<byte> equals = Vector256.Equals(vector, equalsVector);
                Vector256<byte> and = Vector256.BitwiseAnd(equals, oneVector);
                result += Vector256.Sum(and);
            }

            for (int i = 0; i < span.Length; i++)
            {
                bool equals = span[i] == byteToCount;
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        int vector128Count = Vector128<byte>.Count;
        if (Vector128.IsHardwareAccelerated && spanLength >= vector128Count)
        {
            Vector128<byte> equalsVector = Vector128.Create(byteToCount);
            Vector128<byte> oneVector = Vector128.Create((byte)1);
            while (span.Length >= vector128Count)
            {
                Vector128<byte> vector = Vector128.Create(span[..vector128Count]);
                span = span[vector128Count..];
                Vector128<byte> equals = Vector128.Equals(vector, equalsVector);
                Vector128<byte> and = Vector128.BitwiseAnd(equals, oneVector);
                result += Vector128.Sum(and);
            }

            for (int i = 0; i < span.Length; i++)
            {
                bool equals = span[i] == '.';
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        int vector64Count = Vector64<ushort>.Count;
        if (Vector64.IsHardwareAccelerated && spanLength >= vector64Count)
        {
            Vector64<byte> equalsVector = Vector64.Create(byteToCount);
            Vector64<byte> oneVector = Vector64.Create((byte)1);
            while (span.Length >= vector64Count)
            {
                Vector64<byte> vector = Vector64.Create(span[..vector64Count]);
                span = span[vector64Count..];
                Vector64<byte> equals = Vector64.Equals(vector, equalsVector);
                Vector64<byte> and = Vector64.BitwiseAnd(equals, oneVector);
                result += Vector64.Sum(and);
            }

            for (int i = 0; i < span.Length; i++)
            {
                bool equals = span[i] == '.';
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }

        ref byte firstByte = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            bool equals = Unsafe.Add(ref firstByte, i) == byteToCount;
            result += Unsafe.As<bool, byte>(ref equals);
        }

        return result;
    }

    public static int RegexEscape(ReadOnlySpan<char> input, Span<char> escapedInput)
    {
        // chars to escape: "\\*+?|{[()^$. "
        ValueStringBuilder builder = escapedInput;
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

    public static int Join(Span<string> strings, char separator, Span<char> result)
    {
        return Join((ReadOnlySpan<string>)strings, separator, result);
    }

    public static int Join(ReadOnlySpan<string> strings, char separator, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int stringsLengthMinus1 = strings.Length - 1;
        ref string firstString = ref MemoryMarshal.GetReference(strings);
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            string str = Unsafe.Add(ref firstString, i);
            builder.Append(str);
            builder.Append(separator);
        }

        string lastString = Unsafe.Add(ref firstString, stringsLengthMinus1);
        builder.Append(lastString);
        return builder.Length;
    }

    public static int Join(Span<ReadOnlyMemory<char>> strings, char separator, Span<char> result)
    {
        return Join((ReadOnlySpan<ReadOnlyMemory<char>>)strings, separator, result);
    }

    public static int Join(ReadOnlySpan<ReadOnlyMemory<char>> strings, char separator, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int stringsLengthMinus1 = strings.Length - 1;
        ref ReadOnlyMemory<char> firstString = ref MemoryMarshal.GetReference(strings);
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            ReadOnlyMemory<char> str = Unsafe.Add(ref firstString, i);
            builder.Append(str.Span);
            builder.Append(separator);
        }

        ReadOnlyMemory<char> lastString = Unsafe.Add(ref firstString, stringsLengthMinus1);
        builder.Append(lastString.Span);
        return builder.Length;
    }

    public static int Join(Span<string> strings, ReadOnlySpan<char> separator, Span<char> result)
    {
        return Join((ReadOnlySpan<string>)strings, separator, result);
    }

    public static int Join(ReadOnlySpan<string> strings, ReadOnlySpan<char> separator, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int stringsLengthMinus1 = strings.Length - 1;
        ref string firstString = ref MemoryMarshal.GetReference(strings);
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            string str = Unsafe.Add(ref firstString, i);
            builder.Append(str, separator);
        }

        string lastString = Unsafe.Add(ref firstString, stringsLengthMinus1);
        builder.Append(lastString);
        return builder.Length;
    }

    public static int Join(Span<ReadOnlyMemory<char>> strings, ReadOnlySpan<char> separator, Span<char> result)
    {
        return Join((ReadOnlySpan<ReadOnlyMemory<char>>)strings, separator, result);
    }

    public static int Join(ReadOnlySpan<ReadOnlyMemory<char>> strings, ReadOnlySpan<char> separator, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int stringsLengthMinus1 = strings.Length - 1;
        ref ReadOnlyMemory<char> firstString = ref MemoryMarshal.GetReference(strings);
        for (int i = 0; i < stringsLengthMinus1; i++)
        {
            ReadOnlyMemory<char> str = Unsafe.Add(ref firstString, i);
            builder.Append(str.Span, separator);
        }

        ReadOnlyMemory<char> lastString = Unsafe.Add(ref firstString, stringsLengthMinus1);
        builder.Append(lastString.Span);
        return builder.Length;
    }

    public static int Join(Span<char> chars, char separator, Span<char> result)
    {
        return Join((ReadOnlySpan<char>)chars, separator, result);
    }

    public static int Join(ReadOnlySpan<char> chars, char separator, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int charsLengthMinus1 = chars.Length - 1;
        ref char firstChar = ref MemoryMarshal.GetReference(chars);
        for (int i = 0; i < charsLengthMinus1; i++)
        {
            char c = Unsafe.Add(ref firstChar, i);
            builder.Append(c, separator);
        }

        char lastChar = Unsafe.Add(ref firstChar, charsLengthMinus1);
        builder.Append(lastChar);
        return builder.Length;
    }

    public static int Join(Span<char> chars, ReadOnlySpan<char> separator, Span<char> result)
    {
        return Join((ReadOnlySpan<char>)chars, separator, result);
    }

    public static int Join(ReadOnlySpan<char> chars, ReadOnlySpan<char> separator, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int charsLengthMinus1 = chars.Length - 1;
        ref char firstChar = ref MemoryMarshal.GetReference(chars);
        for (int i = 0; i < charsLengthMinus1; i++)
        {
            char c = Unsafe.Add(ref firstChar, i);
            builder.Append(c);
            builder.Append(separator);
        }

        char lastChar = Unsafe.Add(ref firstChar, charsLengthMinus1);
        builder.Append(lastChar);
        return builder.Length;
    }

    public static int Concat(Span<string> strings, Span<char> result)
    {
        return Concat((ReadOnlySpan<string>)strings, result);
    }

    public static int Concat(ReadOnlySpan<string> strings, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int stringsLength = strings.Length;
        ref string firstString = ref MemoryMarshal.GetReference(strings);
        for (int i = 0; i < stringsLength; i++)
        {
            string str = Unsafe.Add(ref firstString, i);
            builder.Append(str);
        }

        return builder.Length;
    }

    public static int Concat(Span<ReadOnlyMemory<char>> strings, Span<char> result)
    {
        return Concat((ReadOnlySpan<ReadOnlyMemory<char>>)strings, result);
    }

    public static int Concat(ReadOnlySpan<ReadOnlyMemory<char>> strings, Span<char> result)
    {
        ValueStringBuilder builder = result;
        int stringsLength = strings.Length;
        ref ReadOnlyMemory<char> firstString = ref MemoryMarshal.GetReference(strings);
        for (int i = 0; i < stringsLength; i++)
        {
            ReadOnlyMemory<char> str = Unsafe.Add(ref firstString, i);
            builder.Append(str.Span);
        }

        return builder.Length;
    }

    [Pure]
    public static ReadOnlySpan<byte> AsByteSpan(this string? str)
    {
        if (str is null || str.Length == 0)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        ref char firstChar = ref MemoryMarshal.GetReference((ReadOnlySpan<char>)str);
        return MemoryMarshal.CreateSpan(ref Unsafe.As<char, byte>(ref firstChar), str.Length << 1);
    }
}
