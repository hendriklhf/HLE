using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE;

public static class Files
{
    public static void ReadBytes(string filePath, PoolBufferWriter<byte> bufferWriter)
    {
        using FileStream fileStream = File.OpenRead(filePath);
        int bytesRead = fileStream.Read(bufferWriter.GetSpan(1000));
        bufferWriter.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = fileStream.Read(bufferWriter.GetSpan(1000));
            bufferWriter.Advance(bytesRead);
        }
    }

    public static async ValueTask ReadBytesAsync(string filePath, PoolBufferWriter<byte> bufferWriter)
    {
        await using FileStream fileStream = File.OpenRead(filePath);
        int bytesRead = await fileStream.ReadAsync(bufferWriter.GetMemory(1000));
        bufferWriter.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = await fileStream.ReadAsync(bufferWriter.GetMemory(1000));
            bufferWriter.Advance(bytesRead);
        }
    }

    [Pure]
    public static string ReadString(string filePath, Encoding fileEncoding)
    {
        using PoolBufferWriter<byte> bufferWriter = new(2500, 5000);
        ReadBytes(filePath, bufferWriter);
        return fileEncoding.GetString(bufferWriter.WrittenSpan);
    }

    [Pure]
    public static async ValueTask<string> ReadStringAsync(string filePath, Encoding fileEncoding)
    {
        using PoolBufferWriter<byte> bufferWriter = new(2500, 5000);
        await ReadBytesAsync(filePath, bufferWriter);
        return fileEncoding.GetString(bufferWriter.WrittenSpan);
    }

    public static void WriteBytes(string filePath, ReadOnlySpan<byte> fileBytes)
    {
        using FileStream fileStream = File.OpenWrite(filePath);
        fileStream.Write(fileBytes);
    }

    public static async ValueTask WriteBytesAsync(string filePath, ReadOnlyMemory<byte> fileBytes)
    {
        await using FileStream fileStream = File.OpenWrite(filePath);
        await fileStream.WriteAsync(fileBytes);
    }

    public static void WriteChars(string filePath, ReadOnlySpan<char> fileContent, Encoding fileEncoding)
    {
        int byteCount = fileEncoding.GetByteCount(fileContent);
        if (!MemoryHelper.UseStackAlloc<byte>(byteCount))
        {
            using RentedArray<byte> byteArrayBuffer = ArrayPool<byte>.Shared.Rent(byteCount);
            fileEncoding.GetBytes(fileContent, byteArrayBuffer);
            WriteBytes(filePath, byteArrayBuffer);
            return;
        }

        Span<byte> byteBuffer = stackalloc byte[byteCount];
        fileEncoding.GetBytes(fileContent, byteBuffer);
        WriteBytes(filePath, byteBuffer);
    }

    public static async ValueTask WriteCharsAsync(string filePath, ReadOnlyMemory<char> fileContent, Encoding fileEncoding)
    {
        int byteCount = fileEncoding.GetByteCount(fileContent.Span);
        using RentedArray<byte> byteBuffer = ArrayPool<byte>.Shared.Rent(byteCount);
        fileEncoding.GetBytes(fileContent.Span, byteBuffer);
        await WriteBytesAsync(filePath, byteBuffer);
    }
}
