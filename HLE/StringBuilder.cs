using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HLE;

[DebuggerDisplay("\"{ToString()}\"")]
public ref struct StringBuilder
{
    public readonly char this[int index]
    {
        get => _buffer[index];
        set => _buffer[index] = value;
    }

    public int Length { get; private set; }

    public readonly int Capacity => _buffer.Length;

    private readonly Span<char> _buffer = Span<char>.Empty;

    public StringBuilder()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span)
    {
        span.CopyTo(_buffer[Length..]);
        Length += span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2)
    {
        Append(span);
        span2.CopyTo(_buffer[Length..]);
        Length += span2.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3)
    {
        Append(span, span2);
        span3.CopyTo(_buffer[Length..]);
        Length += span3.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3, ReadOnlySpan<char> span4)
    {
        Append(span, span2, span3);
        span4.CopyTo(_buffer[Length..]);
        Length += span4.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3, ReadOnlySpan<char> span4, ReadOnlySpan<char> span5)
    {
        Append(span, span2, span3, span4);
        span5.CopyTo(_buffer[Length..]);
        Length += span5.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3, ReadOnlySpan<char> span4, ReadOnlySpan<char> span5, ReadOnlySpan<char> span6)
    {
        Append(span, span2, span3, span4, span5);
        span6.CopyTo(_buffer[Length..]);
        Length += span6.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3, ReadOnlySpan<char> span4, ReadOnlySpan<char> span5, ReadOnlySpan<char> span6, ReadOnlySpan<char> span7)
    {
        Append(span, span2, span3, span4, span5, span6);
        span7.CopyTo(_buffer[Length..]);
        Length += span7.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3, ReadOnlySpan<char> span4, ReadOnlySpan<char> span5, ReadOnlySpan<char> span6, ReadOnlySpan<char> span7, ReadOnlySpan<char> span8)
    {
        Append(span, span2, span3, span4, span5, span6, span7);
        span8.CopyTo(_buffer[Length..]);
        Length += span8.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3, ReadOnlySpan<char> span4, ReadOnlySpan<char> span5, ReadOnlySpan<char> span6, ReadOnlySpan<char> span7, ReadOnlySpan<char> span8,
        ReadOnlySpan<char> span9)
    {
        Append(span, span2, span3, span4, span5, span6, span7, span8);
        span9.CopyTo(_buffer[Length..]);
        Length += span9.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> span, ReadOnlySpan<char> span2, ReadOnlySpan<char> span3, ReadOnlySpan<char> span4, ReadOnlySpan<char> span5, ReadOnlySpan<char> span6, ReadOnlySpan<char> span7, ReadOnlySpan<char> span8,
        ReadOnlySpan<char> span9, ReadOnlySpan<char> span10)
    {
        Append(span, span2, span3, span4, span5, span6, span7, span8, span9);
        span10.CopyTo(_buffer[Length..]);
        Length += span10.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c) => _buffer[Length++] = c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2)
    {
        Append(c);
        _buffer[Length++] = c2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3)
    {
        Append(c, c2);
        _buffer[Length++] = c3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4)
    {
        Append(c, c2, c3);
        _buffer[Length++] = c4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5)
    {
        Append(c, c2, c3, c4);
        _buffer[Length++] = c5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6)
    {
        Append(c, c2, c3, c4, c5);
        _buffer[Length++] = c6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7)
    {
        Append(c, c2, c3, c4, c5, c6);
        _buffer[Length++] = c7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8)
    {
        Append(c, c2, c3, c4, c5, c6, c7);
        _buffer[Length++] = c8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8, char c9)
    {
        Append(c, c2, c3, c4, c5, c6, c7, c8);
        _buffer[Length++] = c9;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8, char c9, char c10)
    {
        Append(c, c2, c3, c4, c5, c6, c7, c8, c9);
        _buffer[Length++] = c10;
    }

    public void Remove(int index)
    {
        _buffer[(index + 1)..Length].CopyTo(_buffer[index..]);
        Length--;
    }

    public override string ToString()
    {
        return new(_buffer[..Length]);
    }

    public static implicit operator StringBuilder(Span<char> buffer) => new(buffer);
}
