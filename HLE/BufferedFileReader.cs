using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Strings;

namespace HLE;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{FilePath}\"")]
public readonly struct BufferedFileReader : IEquatable<BufferedFileReader>
{
    public string FilePath { get; }

    public BufferedFileReader(ReadOnlySpan<char> filePath)
    {
        FilePath = StringPool.Shared.GetOrAdd(filePath);
    }

    public BufferedFileReader(string filePath)
    {
        FilePath = filePath;
    }

    public void ReadBytes<TWriter>(TWriter writer) where TWriter : IBufferWriter<byte>
    {
        using FileStream fileStream = File.OpenRead(FilePath);
        if (fileStream.Length > int.MaxValue)
        {
            ThrowFileSizeExceedsInt32MaxValue();
        }

        int bytesRead = fileStream.Read(writer.GetSpan((int)fileStream.Length));
        Debug.Assert(bytesRead == fileStream.Length);
        writer.Advance(bytesRead);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowFileSizeExceedsInt32MaxValue()
        => throw new NotSupportedException($"The file size exceeds the the maximum {typeof(int)} value.");

    public async ValueTask ReadBytesAsync<TWriter>(TWriter writer) where TWriter : IBufferWriter<byte>
    {
        await using FileStream fileStream = File.OpenRead(FilePath);
        if (fileStream.Length > int.MaxValue)
        {
            ThrowFileSizeExceedsInt32MaxValue();
        }

        int bytesRead = await fileStream.ReadAsync(writer.GetMemory((int)fileStream.Length));
        Debug.Assert(bytesRead == fileStream.Length);
        writer.Advance(bytesRead);
    }

    public void ReadChars<TWriter>(TWriter writer, Encoding fileEncoding) where TWriter : IBufferWriter<char>
    {
        using PooledBufferWriter<byte> byteWriter = new();
        ReadBytes(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    public async ValueTask ReadCharsAsync<TWriter>(TWriter writer, Encoding fileEncoding) where TWriter : IBufferWriter<char>
    {
        using PooledBufferWriter<byte> byteWriter = new();
        await ReadBytesAsync(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, writer.GetSpan(charCount));
        writer.Advance(charsWritten);
    }

    public bool Equals(BufferedFileReader other)
    {
        return FilePath == other.FilePath;
    }

    public override bool Equals(object? obj)
    {
        return obj is BufferedFileReader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return FilePath.GetHashCode();
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
