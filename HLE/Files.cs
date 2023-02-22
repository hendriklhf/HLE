using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using HLE.Memory;

namespace HLE;

public static class Files
{
    public static void ReadBytes(string filePath, IBufferWriter<byte> bufferWriter)
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

    [Pure]
    public static string ReadString(string filePath, Encoding fileEncoding)
    {
        using PoolBufferWriter<byte> bufferWriter = new(2500, 5000);
        ReadBytes(filePath, bufferWriter);
        return fileEncoding.GetString(bufferWriter.WrittenSpan);
    }

    public static void WriteBytes(string filePath, ReadOnlySpan<byte> fileBytes)
    {
        using FileStream fileStream = File.OpenWrite(filePath);
        fileStream.Write(fileBytes);
    }

    public static void WriteString(string filePath, string fileContent, Encoding fileEncoding)
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
}
