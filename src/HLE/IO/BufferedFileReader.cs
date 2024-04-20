using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.IO;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[method: MustDisposeResource]
[DebuggerDisplay("\"{FilePath}\"")]
public struct BufferedFileReader(string filePath) : IDisposable, IEquatable<BufferedFileReader>
{
    public string FilePath { get; } = filePath;

    private SafeFileHandle? _fileHandle;
    private long _size = UninitializedSize;

    private const long UninitializedSize = -1;
    private const FileMode HandleMode = FileMode.Open;
    private const FileAccess HandleAccess = FileAccess.Read;
    private const FileShare HandleShare = FileShare.Read;
    private const FileOptions HandleOptions = FileOptions.SequentialScan;

    public void Dispose()
    {
        _fileHandle?.Dispose();

        _fileHandle = null;
        _size = UninitializedSize;
    }

    [Pure]
    public long GetFileSize()
    {
        if (_size != UninitializedSize)
        {
            return _size;
        }

        _size = RandomAccess.GetLength(OpenHandleIfNotOpen());
        return _size;
    }

    public int ReadBytes(Span<byte> buffer, long fileOffset) => RandomAccess.Read(OpenHandleIfNotOpen(), buffer, fileOffset);

    // ReSharper disable once InconsistentNaming
    public ValueTask<int> ReadBytesAsync(Memory<byte> buffer, long fileOffset) => RandomAccess.ReadAsync(OpenHandleIfNotOpen(), buffer, fileOffset);

    public void ReadBytes<TWriter>(TWriter byteWriter) where TWriter : IBufferWriter<byte>
    {
        SafeFileHandle fileHandle = OpenHandleIfNotOpen();
        int fileSize = GetFileSizeInt32(fileHandle);
        int bytesRead = RandomAccess.Read(fileHandle, byteWriter.GetSpan(fileSize), 0);
        byteWriter.Advance(bytesRead);
    }

    public async ValueTask ReadBytesAsync<TWriter>(TWriter byteWriter) where TWriter : IBufferWriter<byte>
    {
        SafeFileHandle handle = OpenHandleIfNotOpen();
        int fileSize = GetFileSizeInt32(handle);
        int bytesRead = await RandomAccess.ReadAsync(handle, byteWriter.GetMemory(fileSize), 0);
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

    public void ReadLines<TWriter>(TWriter lines, Encoding fileEncoding) where TWriter : IBufferWriter<string>
    {
        using PooledBufferWriter<char> charsWriter = new();
        ReadChars(charsWriter, fileEncoding);
        ReadLinesCore(lines, charsWriter.WrittenSpan);
    }

    public async ValueTask ReadLinesAsync<TWriter>(TWriter lines, Encoding fileEncoding) where TWriter : IBufferWriter<string>
    {
        using PooledBufferWriter<char> charsWriter = new();
        await ReadCharsAsync(charsWriter, fileEncoding);
        ReadLinesCore(lines, charsWriter.WrittenSpan);
    }

    private static void ReadLinesCore<TWriter>(TWriter lines, ReadOnlySpan<char> chars) where TWriter : IBufferWriter<string>
    {
        do
        {
            int indexOfNewLine = chars.IndexOfAny('\r', '\n');
            if (indexOfNewLine < 0)
            {
                break;
            }

            string line = new(chars[..indexOfNewLine]);
            if (typeof(TWriter) == typeof(PooledBufferWriter<string>))
            {
                Unsafe.As<PooledBufferWriter<string>>(lines).Write(line);
            }
            else
            {
                lines.GetSpan(1)[0] = line;
                lines.Advance(1);
            }

            chars = chars[indexOfNewLine..];
            int skipCount = 1;
            if (chars.StartsWith("\r\n"))
            {
                skipCount++;
            }

            chars = chars[skipCount..];
        }
        while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SafeFileHandle OpenHandleIfNotOpen()
    {
        SafeFileHandle? fileHandle = _fileHandle;
        if (fileHandle is { IsClosed: false })
        {
            return fileHandle;
        }

        fileHandle?.Dispose();
        fileHandle = File.OpenHandle(FilePath, HandleMode, HandleAccess, HandleShare, HandleOptions);
        _fileHandle = fileHandle;
        return fileHandle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFileSizeInt32(SafeFileHandle fileHandle)
    {
        long fileSize = RandomAccess.GetLength(fileHandle);
        EnsureFileSizeDoesntExceedMaxArrayLength(fileSize);
        return (int)fileSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureFileSizeDoesntExceedMaxArrayLength(long fileSize)
    {
        if (fileSize > Array.MaxLength)
        {
            ThrowFileSizeExceedsMaxArrayLength();
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowFileSizeExceedsMaxArrayLength()
        => throw new NotSupportedException($"The file size exceeds the maximum array length ({Array.MaxLength}).");

    [Pure]
    public readonly bool Equals(BufferedFileReader other) => FilePath == other.FilePath && _fileHandle?.Equals(other._fileHandle) == true;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is BufferedFileReader other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => FilePath.GetHashCode();

    public static bool operator ==(BufferedFileReader left, BufferedFileReader right) => left.Equals(right);

    public static bool operator !=(BufferedFileReader left, BufferedFileReader right) => !left.Equals(right);
}
