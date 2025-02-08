using System;
using System.Collections.Generic;
using System.Diagnostics;
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

public sealed class PooledMemoryStream(int capacity) :
    Stream,
    IEquatable<PooledMemoryStream>,
    IMemoryProvider<byte>,
    ISpanProvider<byte>,
    ICollectionProvider<byte>,
    ICopyable<byte>,
    IIndexable<byte>
{
    public override bool CanRead => _buffer is not null;

    public override bool CanSeek => _buffer is not null;

    public override bool CanWrite => _buffer is not null;

    public override long Length => (uint)_length;

    int ICountable.Count
    {
        get
        {
            Debug.Assert(Length <= int.MaxValue);
            return (int)Length;
        }
    }

    public byte this[Index index] => ((IIndexable<byte>)this)[index.GetOffset(_length)];

    byte IIndexable<byte>.this[int index]
    {
        get
        {
            Debug.Assert(Length <= int.MaxValue);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)Length, (uint)index);
            return ArrayMarshal.GetUnsafeElementAt(GetBuffer(), index);
        }
    }

    public override long Position
    {
        get => (uint)_position;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)value, (uint)_length);
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
        byte[]? buffer = _buffer;
        if (buffer is null)
        {
            return;
        }

        _buffer = null;
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

        SpanHelpers.Copy(source, ref destination);
    }

    public unsafe void CopyTo(byte* destination)
    {
        Span<byte> source = AsSpan();
        if (source.Length == 0)
        {
            return;
        }

        SpanHelpers.Copy(source, destination);
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
        SpanHelpers.Copy(source, result);
        return result;
    }

    [Pure]
    public byte[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public byte[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public byte[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<byte> ToList() => AsSpan().ToList();

    [Pure]
    public List<byte> ToList(int start) => AsSpan().ToList(start..);

    [Pure]
    public List<byte> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public List<byte> ToList(Range range) => AsSpan().ToList(range);

    [Pure]
    public Span<byte> AsSpan() => GetBuffer().AsSpanUnsafe(.._length);

    [Pure]
    public Span<byte> AsSpan(int start) => Slicer.Slice(ref GetPositionalReference(), _length, start);

    [Pure]
    public Span<byte> AsSpan(int start, int length) => Slicer.Slice(ref GetPositionalReference(), _length, start, length);

    [Pure]
    public Span<byte> AsSpan(Range range) => Slicer.Slice(ref GetPositionalReference(), _length, range);

    [Pure]
    public Memory<byte> AsMemory() => GetBuffer().AsMemory(.._length);

    [Pure]
    public Memory<byte> AsMemory(int start) => AsMemory()[start..];

    [Pure]
    public Memory<byte> AsMemory(int start, int length) => AsMemory().Slice(start, length);

    [Pure]
    public Memory<byte> AsMemory(Range range) => AsMemory()[range];

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledMemoryStream? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledMemoryStream? left, PooledMemoryStream? right) => Equals(left, right);

    public static bool operator !=(PooledMemoryStream? left, PooledMemoryStream? right) => !(left == right);
}
