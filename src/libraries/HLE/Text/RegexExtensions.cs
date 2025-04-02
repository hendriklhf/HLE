using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Text;

public static class RegexExtensions
{
    [Pure]
    public static bool IsMatch(this Regex regex, ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        int charCount;

        int maxCharCount = encoding.GetMaxCharCount(bytes.Length);
        if (!MemoryHelpers.UseStackalloc<char>(maxCharCount))
        {
            char[] rentedChars = ArrayPool<char>.Shared.Rent(maxCharCount);
            charCount = encoding.GetChars(bytes, rentedChars);
            bool result = regex.IsMatch(rentedChars.AsSpanUnsafe(..charCount));
            ArrayPool<char>.Shared.Return(rentedChars);
            return result;
        }

        Span<char> chars = stackalloc char[maxCharCount];
        charCount = encoding.GetChars(bytes, chars);
        return regex.IsMatch(chars.SliceUnsafe(..charCount));
    }

    [Pure]
    public static int Count(this Regex regex, ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        int charCount;

        int maxCharCount = encoding.GetMaxCharCount(bytes.Length);
        if (!MemoryHelpers.UseStackalloc<char>(maxCharCount))
        {
            char[] rentedChars = ArrayPool<char>.Shared.Rent(maxCharCount);
            charCount = encoding.GetChars(bytes, rentedChars);
            int result = regex.Count(rentedChars.AsSpanUnsafe(..charCount));
            ArrayPool<char>.Shared.Return(rentedChars);
            return result;
        }

        Span<char> chars = stackalloc char[maxCharCount];
        charCount = encoding.GetChars(bytes, chars);
        return regex.Count(chars.SliceUnsafe(..charCount));
    }

    public static async Task<int> CountAsync(this Regex regex, Stream stream, Encoding encoding)
    {
        using PooledBufferWriter<char> chars = await GetCharsAsync(stream, encoding).ConfigureAwait(false);
        return regex.Count(chars.WrittenSpan);
    }

    public static async Task<bool> IsMatchAsync(this Regex regex, Stream stream, Encoding encoding)
    {
        using PooledBufferWriter<char> chars = await GetCharsAsync(stream, encoding).ConfigureAwait(false);
        return regex.IsMatch(chars.WrittenSpan);
    }

    private static async Task<PooledBufferWriter<char>> GetCharsAsync(Stream stream, Encoding encoding)
    {
        if (stream.Length > int.MaxValue)
        {
            ThrowHelper.ThrowInvalidOperationException("The stream's length exceeds the maximum buffer length.");
        }

        int streamLength = (int)stream.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
        PooledBufferWriter<char> charsWriter = new(streamLength);
        while (stream.Position != stream.Length)
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
            Span<char> charsBuffer = charsWriter.GetSpan(encoding.GetMaxCharCount(bytesRead));
            int charCount = encoding.GetChars(buffer.AsSpanUnsafe(..bytesRead), charsBuffer);
            charsWriter.Advance(charCount);
        }

        ArrayPool<byte>.Shared.Return(buffer);

        return charsWriter;
    }
}
