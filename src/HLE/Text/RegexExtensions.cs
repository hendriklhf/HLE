using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE.Text;

public static class RegexExtensions
{
    [Pure]
    public static bool IsMatch(this Regex regex, ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        using RentedArray<char> chars = ArrayPool<char>.Shared.RentAsRentedArray(encoding.GetMaxCharCount(bytes.Length));
        int charCount = encoding.GetChars(bytes, chars.AsSpan());
        return regex.IsMatch(chars[..charCount]);
    }

    [Pure]
    public static int Count(this Regex regex, ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        using RentedArray<char> chars = ArrayPool<char>.Shared.RentAsRentedArray(encoding.GetMaxCharCount(bytes.Length));
        int charCount = encoding.GetChars(bytes, chars.AsSpan());
        return regex.Count(chars[..charCount]);
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
        using RentedArray<byte> buffer = ArrayPool<byte>.Shared.RentAsRentedArray(4096);
        PooledBufferWriter<char> charsWriter = new(streamLength);
        while (stream.Position != stream.Length)
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory());
            Span<char> charsBuffer = charsWriter.GetSpan(encoding.GetMaxCharCount(bytesRead));
            int charCount = encoding.GetChars(buffer[..bytesRead], charsBuffer);
            charsWriter.Advance(charCount);
        }

        return charsWriter;
    }
}
