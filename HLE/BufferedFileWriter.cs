using System;
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
public readonly struct BufferedFileWriter : IEquatable<BufferedFileWriter>
{
    public string FilePath { get; }

    public BufferedFileWriter(ReadOnlySpan<char> filePath) => FilePath = StringPool.Shared.GetOrAdd(filePath);

    public BufferedFileWriter(string filePath) => FilePath = filePath;

    public void WriteBytes(ReadOnlySpan<byte> fileBytes)
        => WriteBytes(fileBytes, false);

    public async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> fileBytes)
        => await WriteBytesAsync(fileBytes, false);

    public void WriteChars(ReadOnlySpan<char> fileContent, Encoding fileEncoding)
        => WriteChars(fileContent, fileEncoding, false);

    public async ValueTask WriteCharsAsync(ReadOnlyMemory<char> fileContent, Encoding fileEncoding)
        => await WriteCharsAsync(fileContent, fileEncoding, false);

    public void AppendBytes(ReadOnlySpan<byte> fileBytes)
        => WriteBytes(fileBytes, true);

    public async ValueTask AppendBytesAsync(ReadOnlyMemory<byte> fileBytes)
        => await WriteBytesAsync(fileBytes, true);

    public void AppendChars(ReadOnlySpan<char> fileContent, Encoding fileEncoding)
        => WriteChars(fileContent, fileEncoding, true);

    public async ValueTask AppendCharsAsync(ReadOnlyMemory<char> fileContent, Encoding fileEncoding)
        => await WriteCharsAsync(fileContent, fileEncoding, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteBytes(ReadOnlySpan<byte> fileBytes, [ConstantExpected] bool append)
    {
        if (!append)
        {
            using FileStream fileStream = File.Create(FilePath);
            fileStream.SetLength(fileBytes.Length);
            fileStream.Position = 0;
            fileStream.Write(fileBytes);
        }
        else
        {
            using FileStream fileStream = File.Open(FilePath, FileMode.Append);
            fileStream.Position = fileStream.Length;
            fileStream.Write(fileBytes);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> fileBytes, [ConstantExpected] bool append)
    {
        if (!append)
        {
            await using FileStream fileStream = File.Create(FilePath);
            fileStream.SetLength(fileBytes.Length);
            fileStream.Position = 0;
            await fileStream.WriteAsync(fileBytes);
        }
        else
        {
            await using FileStream fileStream = File.Open(FilePath, FileMode.Append);
            fileStream.Position = fileStream.Length;
            await fileStream.WriteAsync(fileBytes);
        }
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteChars(ReadOnlySpan<char> fileContent, Encoding fileEncoding, [ConstantExpected] bool append)
    {
        int bytesWritten;
        int byteCount = fileEncoding.GetMaxByteCount(fileContent.Length);
        if (!MemoryHelpers.UseStackAlloc<byte>(byteCount))
        {
            using RentedArray<byte> byteArrayBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);
            bytesWritten = fileEncoding.GetBytes(fileContent, byteArrayBuffer.AsSpan());
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
        using RentedArray<byte> byteBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);
        int bytesWritten = fileEncoding.GetBytes(fileContent.Span, byteBuffer.AsSpan());
        await WriteBytesAsync(byteBuffer.AsMemory(..bytesWritten), append);
    }

    [Pure]
    public bool Equals(BufferedFileWriter other) => FilePath == other.FilePath;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BufferedFileWriter other && Equals(other);

    [Pure]
    public override int GetHashCode() => FilePath.GetHashCode();

    public static bool operator ==(BufferedFileWriter left, BufferedFileWriter right) => left.Equals(right);

    public static bool operator !=(BufferedFileWriter left, BufferedFileWriter right) => !left.Equals(right);
}
