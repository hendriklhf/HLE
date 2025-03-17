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
        char[] chars = ArrayPool<char>.Shared.Rent(encoding.GetMaxCharCount(bytes.Length));
        int charCount = encoding.GetChars(bytes, chars.AsSpan());
        bool result = regex.IsMatch(chars.AsSpanUnsafe(..charCount));
        ArrayPool<char>.Shared.Return(chars);
        return result;
    }

    [Pure]
    public static int Count(this Regex regex, ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        char[] chars = ArrayPool<char>.Shared.Rent(encoding.GetMaxCharCount(bytes.Length));
        int charCount = encoding.GetChars(bytes, chars.AsSpan());
        int result = regex.Count(chars.AsSpanUnsafe(..charCount));
        ArrayPool<char>.Shared.Return(chars);
        return result;
    }

    public static async Task<int> CountAsync(this Regex regex, Stream stream, Encoding encoding)
    {
        using PooledBufferWriter<char> chars = await GetCharsAsync(stream, encoding);
        return regex.Count(chars.WrittenSpan);
    }

    public static async Task<bool> IsMatchAsync(this Regex regex, Stream stream, Encoding encoding)
    {
        using PooledBufferWriter<char> chars = await GetCharsAsync(stream, encoding);
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
            int bytesRead = await stream.ReadAsync(buffer.AsMemory());
            Span<char> charsBuffer = charsWriter.GetSpan(encoding.GetMaxCharCount(bytesRead));
            int charCount = encoding.GetChars(buffer.AsSpanUnsafe(..bytesRead), charsBuffer);
            charsWriter.Advance(charCount);
        }

        ArrayPool<byte>.Shared.Return(buffer);

        return charsWriter;
    }
}
