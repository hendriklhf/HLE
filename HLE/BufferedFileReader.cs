using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Strings;

namespace HLE;

public readonly struct BufferedFileReader : IEquatable<BufferedFileReader>
{
    private readonly string _filePath;

    public BufferedFileReader(ReadOnlySpan<char> filePath)
    {
        _filePath = StringPool.Shared.GetOrAdd(filePath);
    }

    public BufferedFileReader(string filePath)
    {
        _filePath = filePath;
    }

    public void ReadBytes<TWriter>(TWriter writer, int fileSizeHint = 100_000) where TWriter : IBufferWriter<byte>
    {
        using FileStream fileStream = File.OpenRead(_filePath);
        int bytesRead = fileStream.Read(writer.GetSpan(fileSizeHint));
        writer.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = fileStream.Read(writer.GetSpan(fileSizeHint));
            writer.Advance(bytesRead);
        }
    }

    public async ValueTask ReadBytesAsync<TWriter>(TWriter writer, int fileSizeHint = 100_000) where TWriter : IBufferWriter<byte>
    {
        await using FileStream fileStream = File.OpenRead(_filePath);
        int bytesRead = await fileStream.ReadAsync(writer.GetMemory(fileSizeHint));
        writer.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = await fileStream.ReadAsync(writer.GetMemory(fileSizeHint));
            writer.Advance(bytesRead);
        }
    }

    public void ReadChars<TWriter>(TWriter writer, Encoding fileEncoding, int fileSizeHint = 100_000) where TWriter : IBufferWriter<char>
    {
        using PooledBufferWriter<byte> byteWriter = new(fileSizeHint);
        ReadBytes(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    public async ValueTask ReadCharsAsync<TWriter>(TWriter writer, Encoding fileEncoding, int fileSizeHint = 100_000) where TWriter : IBufferWriter<char>
    {
        using PooledBufferWriter<byte> byteWriter = new(fileSizeHint);
        await ReadBytesAsync(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    public bool Equals(BufferedFileReader other)
    {
        return _filePath == other._filePath;
    }

    public override bool Equals(object? obj)
    {
        return obj is BufferedFileReader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _filePath.GetHashCode();
    }

    public static bool operator ==(BufferedFileReader left, BufferedFileReader right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BufferedFileReader left, BufferedFileReader right)
    {
        return !left.Equals(right);
    }
}
