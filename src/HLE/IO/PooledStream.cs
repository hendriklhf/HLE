using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;

namespace HLE.IO;

public sealed class PooledStream(int capacity) :
    Stream,
    IEquatable<PooledStream>,
    IMemoryProvider<byte>,
    ISpanProvider<byte>,
    ICollectionProvider<byte>,
    ICopyable<byte>,
    IIndexable<byte>
{
    public override bool CanRead => _buffer is not null;

    public override bool CanSeek => _buffer is not null;

    public override bool CanWrite => _buffer is not null;

    public override long Length => _length;

    int ICountable.Count
    {
        get
        {
            Debug.Assert(Length <= int.MaxValue);
            return (int)Length;
        }
    }

    byte IIndexable<byte>.this[int index]
    {
        get
        {
            Debug.Assert(Length <= int.MaxValue);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)Length, (uint)index);
            return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), index);
        }
    }

    public override long Position
    {
        get => _position;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, _length);
            _position = (int)value;
        }
    }

    private Span<byte> PositionalBuffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBuffer().AsSpan(_position, _length - _position);
    }

    private ref byte PositionalReference
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref MemoryMarshal.GetReference(PositionalBuffer);
    }

    private byte[]? _buffer = capacity == 0 ? [] : ArrayPool<byte>.Shared.Rent(capacity);
    private int _length;
    private int _position;

    public PooledStream() : this(0)
    {
    }

    public PooledStream(ReadOnlySpan<byte> bytes) : this(bytes.Length)
        => SpanHelpers<byte>.Copy(bytes, _buffer.AsSpan());

    public override void Close()
    {
        byte[]? buffer = _buffer;
        if (buffer is null)
        {
            return;
        }

        ArrayPool<byte>.Shared.Return(buffer);
        _buffer = null;
    }

    public override void SetLength(long value)
    {
        if (value > Array.MaxLength)
        {
            ThrowLengthExceedsMaximumArrayLength();
        }

        int length = (int)value;
        byte[] oldBuffer = GetBuffer();
        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(length);
        SpanHelpers<byte>.Copy(oldBuffer.AsSpanUnsafe(.._length), newBuffer);
        ArrayPool<byte>.Shared.Return(oldBuffer);
        _buffer = newBuffer;
        _length = length;
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
                long position = _position + offset;
                ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)position, (ulong)_length);
                _position = (int)position;
                return position;
            }
            case SeekOrigin.End:
            {
                long position = _length - offset;
                ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)position, (ulong)_length);
                _position = (int)position;
                return position;
            }
            default:
                ThrowHelper.ThrowUnreachableException();
                return default;
        }
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        Span<byte> positionalBuffer = PositionalBuffer;
        if (positionalBuffer.Length == 0)
        {
            return 0;
        }

        Span<byte> bufferToCopy = positionalBuffer.Length > buffer.Length ? positionalBuffer[..buffer.Length] : positionalBuffer;
        SpanHelpers<byte>.Copy(bufferToCopy, buffer);
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
        if (_position == _length)
        {
            return -1;
        }

        _position++;
        return PositionalReference;
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Span<byte> positionalBuffer = PositionalBuffer;
        if (positionalBuffer.Length >= buffer.Length)
        {
            SpanHelpers<byte>.Copy(buffer, positionalBuffer);
        }
        else
        {
            GrowBuffer((uint)buffer.Length);
            SpanHelpers<byte>.Copy(buffer, PositionalBuffer);
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
        if (_position == _length)
        {
            GrowBuffer(1);
        }

        PositionalReference = value;
        _position++;
        if (_position > _length)
        {
            _length++;
        }
    }

    [SkipLocalsInit]
    public override void CopyTo(Stream destination, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        int bytesRead;
        if (!MemoryHelpers.UseStackalloc<byte>(bufferSize))
        {
            using RentedArray<byte> rentedBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(bufferSize);
            bytesRead = Read(rentedBuffer.AsSpan(..bufferSize));
            destination.Write(rentedBuffer.AsSpan(..bytesRead));
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
        int newLength = BufferHelpers.GrowArray(currentLength, (uint)_position + sizeHint - currentLength);
        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newLength);
        SpanHelpers<byte>.Copy(currentBuffer, newBuffer);
        ArrayPool<byte>.Shared.Return(currentBuffer);
        _buffer = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] GetBuffer()
    {
        byte[]? buffer = _buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<PooledStream>();
        }

        return buffer;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowLengthExceedsMaximumArrayLength()
        => throw new InvalidOperationException("The length exceeds the maximum array length.");

    public void CopyTo(List<byte> destination, int offset = 0)
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return;
        }

        CopyWorker<byte> copyWorker = new(source);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(byte[] destination, int offset = 0)
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return;
        }

        CopyWorker<byte> copyWorker = new(source);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<byte> destination)
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return;
        }

        CopyWorker<byte> copyWorker = new(source);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<byte> destination)
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return;
        }

        CopyWorker<byte> copyWorker = new(source);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref byte destination)
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return;
        }

        SpanHelpers<byte>.Copy(source, ref destination);
    }

    public unsafe void CopyTo(byte* destination)
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return;
        }

        SpanHelpers<byte>.Copy(source, destination);
    }

    [Pure]
    public byte[] ToArray()
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return [];
        }

        byte[] result = GC.AllocateUninitializedArray<byte>(source.Length);
        SpanHelpers<byte>.Copy(source, result);
        return result;
    }

    [Pure]
    public byte[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public byte[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public byte[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<byte> ToList()
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return [];
        }

        CopyWorker<byte> copyWorker = new(source);
        List<byte> result = new(source.Length);
        copyWorker.CopyTo(result);
        return result;
    }

    [Pure]
    public Span<byte> AsSpan() => GetBuffer().AsSpanUnsafe(.._length);

    [Pure]
    public Memory<byte> AsMemory() => GetBuffer().AsMemory(.._length);

    ReadOnlyMemory<byte> IReadOnlyMemoryProvider<byte>.GetReadOnlyMemory() => AsMemory();

    Memory<byte> IMemoryProvider<byte>.GetMemory() => AsMemory();

    ReadOnlySpan<byte> IReadOnlySpanProvider<byte>.GetReadOnlySpan() => AsSpan();

    Span<byte> ISpanProvider<byte>.GetSpan() => AsSpan();

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledStream? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledStream? left, PooledStream? right) => Equals(left, right);

    public static bool operator !=(PooledStream? left, PooledStream? right) => !(left == right);
}
