using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HLE.Collections;

namespace HLE.Text;

public interface IStringBuilder :
    ISpanProvider<char>,
    IEnumerable<char>
{
    int Length { get; }

    int Capacity { get; }

    Span<char> WrittenSpan { get; }

    Span<char> FreeBufferSpan { get; }

    void EnsureCapacity(int capacity);

    void Advance(int length);

    void Append(ref PooledInterpolatedStringHandler chars);

    void Append(IEnumerable<char> chars);

    void Append(List<char> chars);

    void Append(char[] chars);

    void Append(string str);

    void Append(ReadOnlySpan<char> chars);

    void Append(char c);

    void Append(char c, int count);

    void Append(IEnumerable<string> strings);

    void Append(List<string> strings);

    void Append(string[] strings);

    void Append(Span<string> strings);

    void Append(ReadOnlySpan<string> strings);

    void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(Int128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(UInt128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(nint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(nuint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(decimal value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null);

    void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null);

    void Append(DateTimeOffset dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null);

    void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] string? format = null);

    void Append(DateOnly dateOnly, [StringSyntax(StringSyntaxAttribute.DateOnlyFormat)] string? format = null);

    void Append(TimeOnly timeOnly, [StringSyntax(StringSyntaxAttribute.TimeOnlyFormat)] string? format = null);

    void Append(Guid guid, [StringSyntax(StringSyntaxAttribute.GuidFormat)] string? format = null);

    void Append<T>(T value, string? format = null);

    void Clear();

    string ToString();

    string ToString(int start);

    string ToString(int start, int length);

    string ToString(Range range);
}
