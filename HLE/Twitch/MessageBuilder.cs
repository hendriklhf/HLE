using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Twitch;

[DebuggerDisplay("{ToString()}")]
public partial struct MessageBuilder : IDisposable, IEquatable<MessageBuilder>
{
    public readonly ref char this[int index] => ref Span[index];

    public readonly ref char this[Index index] => ref Span[index];

    public readonly Span<char> this[Range range] => Span[range];

    public readonly int Length => _length;

    public readonly int Capacity => _buffer.Length;

    public readonly Span<char> Span => _buffer;

    public readonly Memory<char> Memory => _buffer;

    public readonly ReadOnlyMemory<char> Message => Memory[.._length];

    public readonly Span<char> FreeBuffer => Span[_length..];

    public readonly int FreeBufferSize => _buffer.Length - _length;

    private readonly char[] _buffer = Array.Empty<char>();
    private int _length;

    private const ushort _maxChatMessageLength = 500;

    public MessageBuilder(int bufferLength = _maxChatMessageLength)
    {
        _buffer = ArrayPool<char>.Shared.Rent(bufferLength);
    }

    public readonly void Dispose()
    {
        ArrayPool<char>.Shared.Return(_buffer);
    }

    public void Advance(int length)
    {
        if (length < 0)
        {
            throw new ArgumentException($"Parameter {nameof(length)} must be a positive number.", nameof(length));
        }

        _length += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        span.CopyTo(FreeBuffer);
        _length += span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        _buffer[_length++] = c;
    }

    public void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(ISpanFormattable spanFormattable, ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        spanFormattable.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Remove(int index, int length = 1)
    {
        Span[(index + length).._length].CopyTo(Span[index..]);
        _length -= length;
    }

#if NET8_0_OR_GREATER
    public readonly void Replace(char oldChar, char newChar)
    {
        Span[.._length].Replace(oldChar, newChar);
    }
#endif

    public void Clear()
    {
        _length = 0;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
    {
        return new(Span[.._length]);
    }

    public readonly char[] ToCharArray()
    {
        return Message.ToArray();
    }

    [Pure]
    public readonly bool Equals(MessageBuilder builder, StringComparison comparisonType)
    {
        return ((ReadOnlySpan<char>)Span[.._length]).Equals(builder.Span[..builder._length], comparisonType);
    }

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType)
    {
        return ((ReadOnlySpan<char>)Span[.._length]).Equals(str, comparisonType);
    }

    public readonly bool Equals(MessageBuilder other)
    {
        return ReferenceEquals(_buffer, other._buffer) && _length == other._length;
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj is MessageBuilder other && Equals(other);
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_buffer, _length);
    }

    public static bool operator ==(MessageBuilder left, MessageBuilder right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MessageBuilder left, MessageBuilder right)
    {
        return !(left == right);
    }
}
