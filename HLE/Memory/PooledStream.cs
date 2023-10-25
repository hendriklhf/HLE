using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;

namespace HLE.Memory;

internal sealed class PooledStream(int minimumLength) : Stream, IEquatable<PooledStream>, ICopyable<byte>
{
    public override bool CanRead => !_buffer.IsDisposed;

    public override bool CanSeek => !_buffer.IsDisposed;

    public override bool CanWrite => !_buffer.IsDisposed;

    public override long Length => _buffer.Length;

    public override long Position
    {
        get => _position;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((ulong)value, (ulong)Length);
            _position = value;
        }
    }

    private RentedArray<byte> _buffer = ArrayPool<byte>.Shared.RentAsRentedArray(minimumLength);
    private long _position;

    public PooledStream() : this(0)
    {
    }

    public new void Dispose()
    {
        base.Dispose();
        _buffer.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        _buffer.Dispose();
    }

    public override int Read(Span<byte> buffer)
    {
        int readableSize = int.Min((int)Length - (int)Position, buffer.Length);
        CopyWorker<byte>.Copy(_buffer.AsSpan((int)Position, readableSize), buffer);
        Position += readableSize;
        return readableSize;
    }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int bytesRead = Read(buffer.Span);
        return ValueTask.FromResult(bytesRead);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int bytesRead = Read(buffer, offset, count);
        return Task.FromResult(bytesRead);
    }

    [Pure]
    public override int ReadByte() => Position == Length ? -1 : _buffer[(int)Position++];

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        GrowIfNeeded(buffer.Length);

        CopyWorker<byte>.Copy(buffer, _buffer.AsSpan((int)Position..));
        Position += buffer.Length;
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

    public override void WriteByte(byte value)
    {
        if (Position == Length)
        {
            ThrowExceedsMaximumStreamCapacity(1);
        }

        GrowIfNeeded(1);
        _buffer[(int)Position++] = value;
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        destination.Write(_buffer.AsSpan());
        Position += _buffer.Length;
    }

    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => await destination.WriteAsync(_buffer.AsMemory(), cancellationToken);

    public void CopyTo(List<byte> destination, int offset = 0) => throw new NotImplementedException();

    public void CopyTo(byte[] destination, int offset = 0) => throw new NotImplementedException();

    public void CopyTo(Memory<byte> destination) => throw new NotImplementedException();

    public void CopyTo(Span<byte> destination) => throw new NotImplementedException();

    public void CopyTo(ref byte destination) => throw new NotImplementedException();

    public unsafe void CopyTo(byte* destination) => throw new NotImplementedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((ulong)value, (ulong)int.MaxValue);

        using RentedArray<byte> oldBuffer = _buffer;
        _buffer = ArrayPool<byte>.Shared.RentAsRentedArray((int)value);
        if (oldBuffer.Length > _buffer.Length)
        {
            CopyWorker<byte>.Copy(oldBuffer.AsSpan(.._buffer.Length), _buffer.AsSpan());
            return;
        }

        CopyWorker<byte>.Copy(oldBuffer.AsSpan(), _buffer.AsSpan());
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void GrowIfNeeded(int sizeHint)
    {
        int freeSpace = (int)(Length - Position);
        if (freeSpace >= sizeHint)
        {
            return;
        }

        int neededSpace = sizeHint - freeSpace;
        const int maximumRoundedUpValue = 1 << 30;
        int newSize = _buffer.Length == maximumRoundedUpValue ? int.MaxValue : (int)BitOperations.RoundUpToPowerOf2((uint)(_buffer.Length + neededSpace));
        if (newSize < _buffer.Length)
        {
            ThrowExceedsMaximumStreamCapacity(sizeHint);
        }

        using RentedArray<byte> oldBuffer = _buffer;
        _buffer = ArrayPool<byte>.Shared.RentAsRentedArray(newSize);
        CopyWorker<byte>.Copy(oldBuffer.AsSpan(), _buffer.AsSpan());
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowExceedsMaximumStreamCapacity(int sizeHint)
        => throw new InvalidOperationException($"The amount of items ({sizeHint}) that are about to be written to the stream, will exceed the maximum capacity of the stream.");

    [Pure]
    public bool Equals(PooledStream? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledStream? left, PooledStream? right) => Equals(left, right);

    public static bool operator !=(PooledStream? left, PooledStream? right) => !(left == right);
}
