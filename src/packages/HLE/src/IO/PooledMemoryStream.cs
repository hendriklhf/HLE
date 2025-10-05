using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.IO;

internal sealed class PooledMemoryStream(int capacity) :
    Stream,
    IEquatable<PooledMemoryStream>
{
    public override bool CanRead => _buffer is not null;

    public override bool CanSeek => _buffer is not null;

    public override bool CanWrite => _buffer is not null;

    public override long Length => (uint)_length;

    public int Capacity => GetBuffer().Length;

    public override long Position
    {
        get => (uint)_position;
        set
        {
            if (value > _length)
            {
                SetLength(value + 1);
            }

            _position = (int)value;
        }
    }

    private byte[]? _buffer = capacity == 0 ? [] : ArrayPool<byte>.Shared.Rent(capacity);
    private int _length;
    private int _position;

    public PooledMemoryStream() : this(0)
    {
    }

    public PooledMemoryStream(ReadOnlySpan<byte> bytes) : this(bytes.Length)
    {
        SpanHelpers.Copy(bytes, _buffer.AsSpan());
        _length = bytes.Length;
        _position = bytes.Length;
    }

    private Span<byte> GetPositionalBuffer() => GetBuffer().AsSpan(_position, _length - _position);

    private ref byte GetPositionalReference() => ref ArrayMarshal.GetUnsafeElementAt(GetBuffer(), _position);

    public override void Close()
    {
        byte[]? buffer = Interlocked.Exchange(ref _buffer, null);
        if (buffer is null)
        {
            return;
        }

        ArrayPool<byte>.Shared.Return(buffer);
    }

    public override void SetLength(long value)
    {
        if (Length == value)
        {
            return;
        }

        if (value > Array.MaxLength)
        {
            ThrowLengthExceedsMaximumArrayLength();
        }

        int length = (int)value;
        byte[] oldBuffer = GetBuffer();
        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(length);
        SpanHelpers.Copy(oldBuffer.AsSpanUnsafe(.._length), newBuffer);
        ArrayPool<byte>.Shared.Return(oldBuffer);
        _buffer = newBuffer;
        _length = length;

        return;

        [DoesNotReturn]
        static void ThrowLengthExceedsMaximumArrayLength()
            => throw new InvalidOperationException("The length exceeds the maximum array length.");
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)offset, (ulong)_length);
                _position = (int)offset;
                return offset;
            case SeekOrigin.Current:
            {
                long position = (uint)_position + offset;
                ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)position, (ulong)_length);
                _position = (int)position;
                return position;
            }
            case SeekOrigin.End:
            {
                long position = (uint)_length - offset;
                ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)position, (ulong)_length);
                _position = (int)position;
                return position;
            }
            default:
                ThrowHelper.ThrowUnreachableException();
                return 0;
        }
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        Span<byte> positionalBuffer = GetPositionalBuffer();
        if (positionalBuffer.Length == 0)
        {
            return 0;
        }

        Span<byte> bufferToCopy = positionalBuffer.Length > buffer.Length ? positionalBuffer[..buffer.Length] : positionalBuffer;
        SpanHelpers.Copy(bufferToCopy, buffer);
        int bytesCopied = bufferToCopy.Length;
        _position += bytesCopied;
        return bytesCopied;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<int>(cancellationToken);
        }

        int bytesRead = Read(buffer.AsSpan(offset, count));
        return Task.FromResult(bytesRead);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<int>(cancellationToken);
        }

        int bytesRead = Read(buffer.Span);
        return ValueTask.FromResult(bytesRead);
    }

    public override int ReadByte()
    {
        int position = _position;
        if (position == _length)
        {
            return -1;
        }

        int value = ArrayMarshal.GetUnsafeElementAt(GetBuffer(), position);
        _position = position + 1;
        return value;
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Span<byte> positionalBuffer = GetPositionalBuffer();
        if (positionalBuffer.Length >= buffer.Length)
        {
            SpanHelpers.Copy(buffer, positionalBuffer);
        }
        else
        {
            GrowBuffer((uint)buffer.Length);
            SpanHelpers.Copy(buffer, ref GetPositionalReference());
        }

        int position = _position + buffer.Length;
        if (position > _length)
        {
            _length = position;
        }

        _position = position;
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        Write(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    public override void WriteByte(byte value)
    {
        int position = _position;
        if (position == _length)
        {
            GrowBuffer(1);
        }

        GetPositionalReference() = value;

        _position = ++position;
        if (position > _length)
        {
            _length++;
        }
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        int bytesRead;
        if (!MemoryHelpers.UseStackalloc<byte>(bufferSize))
        {
            byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            bytesRead = Read(rentedBuffer.AsSpan(..bufferSize));
            destination.Write(rentedBuffer.AsSpan(..bytesRead));
            ArrayPool<byte>.Shared.Return(rentedBuffer);
            return;
        }

        Span<byte> buffer = stackalloc byte[bufferSize];
        bytesRead = Read(buffer[..bufferSize]);
        destination.Write(buffer[..bytesRead]);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        CopyTo(destination, bufferSize);
        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // slow path
    private void GrowBuffer(uint sizeHint)
    {
        byte[] currentBuffer = GetBuffer();
        uint currentLength = (uint)currentBuffer.Length;
        int newLength = BufferHelpers.GrowArray(currentLength, sizeHint);
        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newLength);
        SpanHelpers.Copy(currentBuffer, newBuffer);

        ArrayPool<byte>.Shared.Return(currentBuffer);
        _buffer = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] GetBuffer()
    {
        byte[]? buffer = _buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<PooledMemoryStream>();
        }

        return buffer;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledMemoryStream? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledMemoryStream? left, PooledMemoryStream? right) => Equals(left, right);

    public static bool operator !=(PooledMemoryStream? left, PooledMemoryStream? right) => !(left == right);
}
