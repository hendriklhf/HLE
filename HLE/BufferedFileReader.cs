using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
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

    public BufferedFileReader(ReadOnlySpan<char> filePath) => FilePath = StringPool.Shared.GetOrAdd(filePath);

    public BufferedFileReader(string filePath) => FilePath = filePath;

    public void ReadBytes<TWriter>(TWriter writer) where TWriter : IBufferWriter<byte>
    {
        using FileStream fileStream = File.OpenRead(FilePath);
        if (fileStream.Length > Array.MaxLength)
        {
            ThrowFileSizeExceedsMaxArrayLength();
        }

        int bytesRead = fileStream.Read(writer.GetSpan((int)fileStream.Length));
        Debug.Assert(bytesRead == fileStream.Length);
        writer.Advance(bytesRead);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowFileSizeExceedsMaxArrayLength()
        => throw new NotSupportedException($"The file size exceeds the the maximum array length ({Array.MaxLength}).");

    public async ValueTask ReadBytesAsync<TWriter>(TWriter byteWriter) where TWriter : IBufferWriter<byte>
    {
        await using FileStream fileStream = File.OpenRead(FilePath);
        if (fileStream.Length > Array.MaxLength)
        {
            ThrowFileSizeExceedsMaxArrayLength();
        }

        int bytesRead = await fileStream.ReadAsync(byteWriter.GetMemory((int)fileStream.Length));
        Debug.Assert(bytesRead == fileStream.Length);
        byteWriter.Advance(bytesRead);
    }

    public void ReadChars<TWriter>(TWriter charWriter, Encoding fileEncoding) where TWriter : IBufferWriter<char>
    {
        using PooledBufferWriter<byte> byteWriter = new();
        ReadBytes(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, charWriter.GetSpan(charCount));
        charWriter.Advance(charsWritten);
    }

    public async ValueTask ReadCharsAsync<TWriter>(TWriter charWriter, Encoding fileEncoding) where TWriter : IBufferWriter<char>
    {
        using PooledBufferWriter<byte> byteWriter = new();
        await ReadBytesAsync(byteWriter);
        int charCount = fileEncoding.GetMaxCharCount(byteWriter.Count);
        int charsWritten = fileEncoding.GetChars(byteWriter.WrittenSpan, charWriter.GetSpan(charCount));
        charWriter.Advance(charsWritten);
    }

    public void ReadLines<TWriter>(TWriter lines) where TWriter : IBufferWriter<string>
    {
        BufferedFileReader reader = new(FilePath);
        using PooledBufferWriter<char> charsWriter = new();
        reader.ReadChars(charsWriter, Encoding.UTF8);

        ReadOnlySpan<char> chars = charsWriter.WrittenSpan;
        while (true)
        {
            int indexOfNewLine = chars.IndexOfAny('\r', '\n');
            if (indexOfNewLine < 0)
            {
                break;
            }

            string line = new(chars[..indexOfNewLine]);
            if (typeof(TWriter) == typeof(PooledBufferWriter<string>))
            {
                Unsafe.As<TWriter, PooledBufferWriter<string>>(ref lines).Write(line);
            }
            else
            {
                lines.GetSpan(1)[0] = line;
                lines.Advance(1);
            }

            chars = chars[indexOfNewLine..];
            int skip = 1;
            if (chars.StartsWith("\r\n"))
            {
                skip++;
            }

            chars = chars[skip..];
        }
    }

    [Pure]
    public bool Equals(BufferedFileReader other) => FilePath == other.FilePath;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BufferedFileReader other && Equals(other);

    [Pure]
    public override int GetHashCode() => FilePath.GetHashCode();

    public static bool operator ==(BufferedFileReader left, BufferedFileReader right) => left.Equals(right);

    public static bool operator !=(BufferedFileReader left, BufferedFileReader right) => !left.Equals(right);
}
