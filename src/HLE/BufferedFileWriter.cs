using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using Microsoft.Win32.SafeHandles;

namespace HLE;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{FilePath}\"")]
public struct BufferedFileWriter(string filePath) : IDisposable, IEquatable<BufferedFileWriter>
{
    public string FilePath { get; } = filePath;

    private SafeFileHandle? _fileHandle;
    private long _size = -1;

    private const FileMode HandleMode = FileMode.OpenOrCreate;
    private const FileAccess HandleAccess = FileAccess.Write;
    private const FileShare HandleShare = FileShare.Write;

    public void Dispose()
    {
        SafeFileHandle? fileHandle = _fileHandle;
        if (fileHandle is null)
        {
            return;
        }

        long size = _size;
        if (size != -1)
        {
            RandomAccess.SetLength(fileHandle, size);
        }

        fileHandle.Dispose();
        _fileHandle = null;
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        OpenHandleIfNotOpen();

        SafeFileHandle fileHandle = _fileHandle;
        long size = GetFileSize(fileHandle, false);

        RandomAccess.Write(fileHandle, bytes, size);
        _size = size + bytes.Length;
    }

    public async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> bytes)
    {
        OpenHandleIfNotOpen();

        SafeFileHandle fileHandle = _fileHandle;
        long size = GetFileSize(fileHandle, false);

        await RandomAccess.WriteAsync(fileHandle, bytes, size);
        _size = size + bytes.Length;
    }

    public void AppendBytes(ReadOnlySpan<byte> bytes)
    {
        OpenHandleIfNotOpen();

        SafeFileHandle fileHandle = _fileHandle;
        long size = GetFileSize(fileHandle, true);

        RandomAccess.Write(fileHandle, bytes, size);
        _size = size + bytes.Length;
    }

    public async ValueTask AppendBytesAsync(ReadOnlyMemory<byte> bytes)
    {
        OpenHandleIfNotOpen();

        SafeFileHandle fileHandle = _fileHandle;
        long size = GetFileSize(fileHandle, true);

        await RandomAccess.WriteAsync(fileHandle, bytes, size);

        _size = size + bytes.Length;
    }

    public void WriteChars(ReadOnlySpan<char> chars, Encoding fileEncoding)
    {
        int maximumByteCount = fileEncoding.GetMaxByteCount(chars.Length);
        using RentedArray<byte> byteBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(maximumByteCount);
        int byteCount = fileEncoding.GetBytes(chars, byteBuffer.AsSpan());
        ReadOnlySpan<byte> bytes = byteBuffer[..byteCount];
        WriteBytes(bytes);
    }

    public void AppendChars(ReadOnlySpan<char> chars, Encoding fileEncoding)
    {
        int maximumByteCount = fileEncoding.GetMaxByteCount(chars.Length);
        using RentedArray<byte> byteBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(maximumByteCount);
        int byteCount = fileEncoding.GetBytes(chars, byteBuffer.AsSpan());
        ReadOnlySpan<byte> bytes = byteBuffer[..byteCount];
        AppendBytes(bytes);
    }

    public async ValueTask WriteCharsAsync(ReadOnlyMemory<char> chars, Encoding fileEncoding)
    {
        int maximumByteCount = fileEncoding.GetMaxByteCount(chars.Length);
        using RentedArray<byte> byteBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(maximumByteCount);
        int byteCount = fileEncoding.GetBytes(chars.Span, byteBuffer.AsSpan());
        ReadOnlyMemory<byte> bytes = byteBuffer.AsMemory(..byteCount);
        await WriteBytesAsync(bytes);
    }

    public async ValueTask AppendCharsAsync(ReadOnlyMemory<char> chars, Encoding fileEncoding)
    {
        int maximumByteCount = fileEncoding.GetMaxByteCount(chars.Length);
        using RentedArray<byte> byteBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(maximumByteCount);
        int byteCount = fileEncoding.GetBytes(chars.Span, byteBuffer.AsSpan());
        ReadOnlyMemory<byte> bytes = byteBuffer.AsMemory(..byteCount);
        await AppendBytesAsync(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly long GetFileSize(SafeFileHandle fileHandle, [ConstantExpected] bool append)
    {
        long size = _size;
        if (size == -1)
        {
            size = append ? RandomAccess.GetLength(fileHandle) : 0;
        }

        return size;
    }

    [MemberNotNull(nameof(_fileHandle))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OpenHandleIfNotOpen()
    {
        if (_fileHandle is { IsClosed: false })
        {
            return;
        }

        _fileHandle?.Dispose();
        _fileHandle = File.OpenHandle(FilePath, HandleMode, HandleAccess, HandleShare);
    }

    [Pure]
    public readonly bool Equals(BufferedFileWriter other) => FilePath == other.FilePath && _fileHandle?.Equals(other._fileHandle) == true;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is BufferedFileWriter other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => FilePath.GetHashCode();

    public static bool operator ==(BufferedFileWriter left, BufferedFileWriter right) => left.Equals(right);

    public static bool operator !=(BufferedFileWriter left, BufferedFileWriter right) => !left.Equals(right);
}
