using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;

namespace HLE.Memory;

public sealed class PooledStream(int capacity) : Stream, IEquatable<PooledStream>
{
    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, _length);
            _position = (int)value;
        }
    }

    private byte[]? _buffer = ArrayPool<byte>.Shared.Rent(capacity);
    private int _length;
    private int _position;

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
        _buffer = newBuffer;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void Flush() => throw new NotImplementedException();

    public override Task FlushAsync(CancellationToken cancellationToken) => base.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override int Read(Span<byte> buffer) => throw new NotImplementedException();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        base.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) =>
        base.ReadAsync(buffer, cancellationToken);

    public override int ReadByte() => base.ReadByte();

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        base.BeginRead(buffer, offset, count, callback, state);

    public override int EndRead(IAsyncResult asyncResult) => base.EndRead(asyncResult);

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotImplementedException();

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        base.WriteAsync(buffer, offset, count, cancellationToken);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
        base.WriteAsync(buffer, cancellationToken);

    public override void WriteByte(byte value) => base.WriteByte(value);

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        base.BeginWrite(buffer, offset, count, callback, state);

    public override void EndWrite(IAsyncResult asyncResult) => base.EndWrite(asyncResult);

    public override void CopyTo(Stream destination, int bufferSize) => base.CopyTo(destination, bufferSize);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) =>
        base.CopyToAsync(destination, bufferSize, cancellationToken);

    private ref byte GetBufferReference() => ref MemoryMarshal.GetArrayDataReference(GetBuffer());

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

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledStream? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledStream? left, PooledStream? right) => Equals(left, right);

    public static bool operator !=(PooledStream? left, PooledStream? right) => !(left == right);
}
