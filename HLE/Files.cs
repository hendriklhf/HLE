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
    public static void ReadBytes<TWriter>(string filePath, TWriter writer) where TWriter : IBufferWriter<byte>
    {
        using FileStream fileStream = File.OpenRead(filePath);
        int bytesRead = fileStream.Read(writer.GetSpan(1000));
        writer.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = fileStream.Read(writer.GetSpan(1000));
            writer.Advance(bytesRead);
        }
    }

    public static async ValueTask ReadBytesAsync<TWriter>(string filePath, TWriter writer) where TWriter : IBufferWriter<byte>
    {
        await using FileStream fileStream = File.OpenRead(filePath);
        int bytesRead = await fileStream.ReadAsync(writer.GetMemory(1000));
        writer.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = await fileStream.ReadAsync(writer.GetMemory(1000));
            writer.Advance(bytesRead);
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
