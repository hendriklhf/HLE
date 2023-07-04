using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE;

public readonly struct BufferedFileWriter
{
    private readonly string _filePath;

    public BufferedFileWriter(string filePath)
    {
        _filePath = filePath;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteBytes(ReadOnlySpan<byte> fileBytes, [ConstantExpected] bool append)
    {
        using FileStream fileStream = File.OpenWrite(_filePath);
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
    private async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> fileBytes, [ConstantExpected] bool append)
    {
        await using FileStream fileStream = File.OpenWrite(_filePath);
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

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteChars(ReadOnlySpan<char> fileContent, Encoding fileEncoding, [ConstantExpected] bool append)
    {
        int bytesWritten;
        int byteCount = fileEncoding.GetMaxByteCount(fileContent.Length);
        if (!MemoryHelper.UseStackAlloc<byte>(byteCount))
        {
            using RentedArray<byte> byteArrayBuffer = new(byteCount);
            bytesWritten = fileEncoding.GetBytes(fileContent, byteArrayBuffer);
            WriteBytes(byteArrayBuffer[..bytesWritten]);
            return;
        }

        Span<byte> byteBuffer = stackalloc byte[byteCount];
        bytesWritten = fileEncoding.GetBytes(fileContent, byteBuffer);
        WriteBytes(byteBuffer[..bytesWritten], append);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask WriteCharsAsync(ReadOnlyMemory<char> fileContent, Encoding fileEncoding, [ConstantExpected] bool append)
    {
        int byteCount = fileEncoding.GetMaxByteCount(fileContent.Length);
        using RentedArray<byte> byteBuffer = new(byteCount);
        int bytesWritten = fileEncoding.GetBytes(fileContent.Span, byteBuffer);
        await WriteBytesAsync(byteBuffer.Memory[..bytesWritten], append);
    }

    public void WriteBytes(ReadOnlySpan<byte> fileBytes)
    {
        WriteBytes(fileBytes, false);
    }

    public async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> fileBytes)
    {
        await WriteBytesAsync(fileBytes, false);
    }

    public void WriteChars(ReadOnlySpan<char> fileContent, Encoding fileEncoding)
    {
        WriteChars(fileContent, fileEncoding, false);
    }

    public async ValueTask WriteCharsAsync(ReadOnlyMemory<char> fileContent, Encoding fileEncoding)
    {
        await WriteCharsAsync(fileContent, fileEncoding, false);
    }

    public void AppendBytes(ReadOnlySpan<byte> fileBytes)
    {
        WriteBytes(fileBytes, true);
    }

    public async ValueTask AppendBytesAsync(ReadOnlyMemory<byte> fileBytes)
    {
        await WriteBytesAsync(fileBytes, true);
    }

    public void AppendChars(ReadOnlySpan<char> fileContent, Encoding fileEncoding)
    {
        WriteChars(fileContent, fileEncoding, true);
    }

    public async ValueTask AppendCharsAsync(ReadOnlyMemory<char> fileContent, Encoding fileEncoding)
    {
        await WriteCharsAsync(fileContent, fileEncoding, true);
    }
}
