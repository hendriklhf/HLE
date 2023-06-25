using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE;

public readonly struct BufferedFileReader
{
    private readonly string _filePath;

    public BufferedFileReader(string filePath)
    {
        _filePath = filePath;
    }

    public void ReadBytes<TWriter>(TWriter writer, int fileSizeHint = 2500) where TWriter : IBufferWriter<byte>
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

    public async ValueTask ReadBytesAsync<TWriter>(TWriter writer, int fileSizeHint = 2500) where TWriter : IBufferWriter<byte>
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

    public void ReadChars<TWriter>(TWriter writer, Encoding fileEncoding, int fileSizeHint = 2500) where TWriter : IBufferWriter<char>
    {
        using PoolBufferWriter<byte> byteWriter = new(fileSizeHint);
        ReadBytes(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    public async ValueTask ReadCharsAsync<TWriter>(TWriter writer, Encoding fileEncoding, int fileSizeHint = 2500) where TWriter : IBufferWriter<char>
    {
        using PoolBufferWriter<byte> byteWriter = new(fileSizeHint);
        await ReadBytesAsync(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    [Pure]
    public string ReadString(Encoding fileEncoding, int fileSizeHint = 2500)
    {
        using PoolBufferWriter<byte> bufferWriter = new(fileSizeHint);
        ReadBytes(bufferWriter);
        return fileEncoding.GetString(bufferWriter.WrittenSpan);
    }

    [Pure]
    public async ValueTask<string> ReadStringAsync(Encoding fileEncoding, int fileSizeHint = 2500)
    {
        using PoolBufferWriter<byte> bufferWriter = new(fileSizeHint);
        await ReadBytesAsync(bufferWriter);
        return fileEncoding.GetString(bufferWriter.WrittenSpan);
    }
}
