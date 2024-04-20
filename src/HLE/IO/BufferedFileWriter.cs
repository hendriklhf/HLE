using System;
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
public struct BufferedFileWriter(string filePath) : IDisposable, IEquatable<BufferedFileWriter>
{
    public string FilePath { get; } = filePath;

    private SafeFileHandle? _fileHandle;
    private long _size = UninitializedSize;

    private const long UninitializedSize = -1;
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
        if (size != UninitializedSize)
        {
            RandomAccess.SetLength(fileHandle, size);
        }

        fileHandle.Dispose();
        _fileHandle = null;
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        SafeFileHandle fileHandle = OpenHandleIfNotOpen();
        long writeOffset = GetWriteOffset(fileHandle, false);
        RandomAccess.Write(fileHandle, bytes, writeOffset);
        _size = writeOffset + bytes.Length;
    }

    public async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> bytes)
    {
        SafeFileHandle fileHandle = OpenHandleIfNotOpen();
        long writeOffset = GetWriteOffset(fileHandle, false);
        await RandomAccess.WriteAsync(fileHandle, bytes, writeOffset);
        _size = writeOffset + bytes.Length;
    }

    public void AppendBytes(ReadOnlySpan<byte> bytes)
    {
        SafeFileHandle fileHandle = OpenHandleIfNotOpen();
        long writeOffset = GetWriteOffset(fileHandle, true);

        RandomAccess.Write(fileHandle, bytes, writeOffset);
        _size = writeOffset + bytes.Length;
    }

    public async ValueTask AppendBytesAsync(ReadOnlyMemory<byte> bytes)
    {
        SafeFileHandle fileHandle = OpenHandleIfNotOpen();
        long writeOffset = GetWriteOffset(fileHandle, true);

        await RandomAccess.WriteAsync(fileHandle, bytes, writeOffset);

        _size = writeOffset + bytes.Length;
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
    private readonly long GetWriteOffset(SafeFileHandle fileHandle, [ConstantExpected] bool append)
    {
        long size = _size;
        if (size == UninitializedSize)
        {
            size = append ? RandomAccess.GetLength(fileHandle) : 0;
        }

        return size;
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
        fileHandle = File.OpenHandle(FilePath, HandleMode, HandleAccess, HandleShare);
        _fileHandle = fileHandle;
        return fileHandle;
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
