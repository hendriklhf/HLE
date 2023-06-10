using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE;

public static class Files
{
    public static void ReadBytes<TWriter>(string filePath, TWriter writer, int fileSizeHint = 2500) where TWriter : IBufferWriter<byte>
    {
        using FileStream fileStream = File.OpenRead(filePath);
        int bytesRead = fileStream.Read(writer.GetSpan(fileSizeHint));
        writer.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = fileStream.Read(writer.GetSpan(fileSizeHint));
            writer.Advance(bytesRead);
        }
    }

    public static async ValueTask ReadBytesAsync<TWriter>(string filePath, TWriter writer, int fileSizeHint = 2500) where TWriter : IBufferWriter<byte>
    {
        await using FileStream fileStream = File.OpenRead(filePath);
        int bytesRead = await fileStream.ReadAsync(writer.GetMemory(fileSizeHint));
        writer.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = await fileStream.ReadAsync(writer.GetMemory(fileSizeHint));
            writer.Advance(bytesRead);
        }
    }

    public static void ReadChars<TWriter>(string filePath, Encoding fileEncoding, TWriter writer, int fileSizeHint = 2500) where TWriter : IBufferWriter<char>
    {
        using PoolBufferWriter<byte> byteWriter = new(fileSizeHint, fileSizeHint << 1);
        ReadBytes(filePath, byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Length);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    public static async ValueTask ReadCharsAsync<TWriter>(string filePath, Encoding fileEncoding, TWriter writer, int fileSizeHint = 2500) where TWriter : IBufferWriter<char>
    {
        using PoolBufferWriter<byte> byteWriter = new(fileSizeHint, fileSizeHint << 1);
        await ReadBytesAsync(filePath, byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Length);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    [Pure]
    public static string ReadString(string filePath, Encoding fileEncoding, int fileSizeHint = 2500)
    {
        using PoolBufferWriter<byte> bufferWriter = new(fileSizeHint, fileSizeHint << 1);
        ReadBytes(filePath, bufferWriter);
        return fileEncoding.GetString(bufferWriter.WrittenSpan);
    }

    [Pure]
    public static async ValueTask<string> ReadStringAsync(string filePath, Encoding fileEncoding, int fileSizeHint = 2500)
    {
        using PoolBufferWriter<byte> bufferWriter = new(fileSizeHint, fileSizeHint << 1);
        await ReadBytesAsync(filePath, bufferWriter);
        return fileEncoding.GetString(bufferWriter.WrittenSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBytes(string filePath, ReadOnlySpan<byte> fileBytes, bool append)
    {
        using FileStream fileStream = File.OpenWrite(filePath);
        if (!append)
        {
            fileStream.SetLength(fileBytes.Length);
            fileStream.Position = 0;
        }
        else
        {
            fileStream.Position = fileStream.Length;
        }

        fileStream.Write(fileBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask WriteBytesAsync(string filePath, ReadOnlyMemory<byte> fileBytes, bool append)
    {
        await using FileStream fileStream = File.OpenWrite(filePath);
        if (!append)
        {
            fileStream.SetLength(fileBytes.Length);
            fileStream.Position = 0;
        }
        else
        {
            fileStream.Position = fileStream.Length;
        }

        await fileStream.WriteAsync(fileBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteChars(string filePath, ReadOnlySpan<char> fileContent, Encoding fileEncoding, bool append)
    {
        int bytesWritten;
        int byteCount = fileEncoding.GetMaxByteCount(fileContent.Length);
        if (!MemoryHelper.UseStackAlloc<byte>(byteCount))
        {
            using RentedArray<byte> byteArrayBuffer = new(byteCount);
            bytesWritten = fileEncoding.GetBytes(fileContent, byteArrayBuffer);
            WriteBytes(filePath, byteArrayBuffer[..bytesWritten]);
            return;
        }

        Span<byte> byteBuffer = stackalloc byte[byteCount];
        bytesWritten = fileEncoding.GetBytes(fileContent, byteBuffer);
        WriteBytes(filePath, byteBuffer[..bytesWritten], append);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask WriteCharsAsync(string filePath, ReadOnlyMemory<char> fileContent, Encoding fileEncoding, bool append)
    {
        int byteCount = fileEncoding.GetMaxByteCount(fileContent.Length);
        using RentedArray<byte> byteBuffer = new(byteCount);
        int bytesWritten = fileEncoding.GetBytes(fileContent.Span, byteBuffer);
        await WriteBytesAsync(filePath, byteBuffer.Memory[..bytesWritten], append);
    }

    public static void WriteBytes(string filePath, ReadOnlySpan<byte> fileBytes)
    {
        WriteBytes(filePath, fileBytes, false);
    }

    public static async ValueTask WriteBytesAsync(string filePath, ReadOnlyMemory<byte> fileBytes)
    {
        await WriteBytesAsync(filePath, fileBytes, false);
    }

    public static void WriteChars(string filePath, ReadOnlySpan<char> fileContent, Encoding fileEncoding)
    {
        WriteChars(filePath, fileContent, fileEncoding, false);
    }

    public static async ValueTask WriteCharsAsync(string filePath, ReadOnlyMemory<char> fileContent, Encoding fileEncoding)
    {
        await WriteCharsAsync(filePath, fileContent, fileEncoding, false);
    }

    public static void AppendBytes(string filePath, ReadOnlySpan<byte> fileBytes)
    {
        WriteBytes(filePath, fileBytes, true);
    }

    public static async ValueTask AppendBytesAsync(string filePath, ReadOnlyMemory<byte> fileBytes)
    {
        await WriteBytesAsync(filePath, fileBytes, true);
    }

    public static void AppendChars(string filePath, ReadOnlySpan<char> fileContent, Encoding fileEncoding)
    {
        WriteChars(filePath, fileContent, fileEncoding, true);
    }

    public static async ValueTask AppendCharsAsync(string filePath, ReadOnlyMemory<char> fileContent, Encoding fileEncoding)
    {
        await WriteCharsAsync(filePath, fileContent, fileEncoding, true);
    }
}
