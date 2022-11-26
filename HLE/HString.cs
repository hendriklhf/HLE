using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace HLE;

[DebuggerDisplay("\"{_string}\" Length = {Length}")]
public sealed class HString : IEnumerable<char>, ICloneable, IConvertible
{
    public char this[int idx]
    {
        get => GetChar(idx);
        set => SetChar(idx, value);
    }

    public char this[Index index]
    {
        get => AsSpan()[index];
        set => SetIndex(index, value);
    }

    public ReadOnlySpan<char> this[Range range]
    {
        get => AsSpan()[range];
        set => SetRange(range, value);
    }

    public int Length => _string.Length;

    public static HString Empty { get; } = new(string.Empty);

    private readonly string _string;

    public HString(char[]? chars)
    {
        _string = new(chars ?? Array.Empty<char>());
    }

    public HString(HString? hString)
    {
        _string = new(hString?._string ?? string.Empty);
    }

    public HString(string? str)
    {
        _string = new(str ?? string.Empty);
    }

    public HString(ReadOnlySpan<char> span)
    {
        _string = new(span);
    }

    public ReadOnlySpan<char> AsSpan() => _string;

    public bool Contains(char value) => AsSpan().Contains(value);

    public bool Contains(string value, StringComparison comparisonType = default) => AsSpan().Contains(value, comparisonType);

    public HString[] Split(char separator = ' ')
    {
        ReadOnlySpan<char> span = _string;
        Range[] ranges = span.GetRangesOfSplit(separator);
        HString[] result = new HString[ranges.Length];
        for (int i = 0; i < ranges.Length; i++)
        {
            result[i] = new(span[ranges[i]]);
        }

        return result;
    }

    public HString[] Split(ReadOnlySpan<char> separator)
    {
        ReadOnlySpan<char> span = _string;
        Range[] ranges = span.GetRangesOfSplit(separator);
        HString[] result = new HString[ranges.Length];
        for (int i = 0; i < ranges.Length; i++)
        {
            result[i] = new(span[ranges[i]]);
        }

        return result;
    }

    public bool StartsWith(char value) => AsSpan()[0] == value;

    public bool StartsWith(string value) => AsSpan().StartsWith(value);

    public bool EndsWith(char value) => AsSpan()[^1] == value;

    public bool EndsWith(string value) => AsSpan().EndsWith(value);

    public HString ToLower() => _string.ToLower();

    public HString ToUpper() => _string.ToUpper();

    public char[] ToCharArray() => _string.ToCharArray();

    private void SetChar(int idx, char value)
    {
        if (idx < 0 || idx >= _string.Length)
        {
            throw new IndexOutOfRangeException($"Out of range for index {idx}. Array has a length of {Length}.");
        }

        Span<char> span = _string.AsSpan();
        span[idx] = value;
    }

    private char GetChar(int idx)
    {
        if (idx < 0 || idx >= Length)
        {
            throw new IndexOutOfRangeException($"Out of range for index {idx}. Array has a length of {Length}.");
        }

        return AsSpan()[idx];
    }

    private void SetIndex(Index index, char value)
    {
        int idx = index.IsFromEnd ? Length - index.Value - 1 : index.Value;
        SetChar(idx, value);
    }

    private void SetRange(Range range, ReadOnlySpan<char> value)
    {
        int start = range.Start.Value;
        int end = range.End.IsFromEnd ? Length - range.End.Value - 1 : range.End.Value;

        if (start > end)
        {
            throw new InvalidOperationException("The starting index can't be larger than the ending index.");
        }

        int rangeLength = end - start;
        if (rangeLength != value.Length)
        {
            throw new InvalidOperationException($"Parameter {nameof(range)} and {nameof(value)} need to have the same length. Length of {nameof(range)} is {rangeLength} and length of {nameof(value)} is {value.Length}");
        }

        for (int i = start; i < end; i++)
        {
            SetChar(i, value[i - start]);
        }
    }

    public static implicit operator string(HString? h)
    {
        return h?._string ?? string.Empty;
    }

    public static implicit operator HString(string? str)
    {
        return new(str);
    }

    public static implicit operator HString(char[]? chars)
    {
        return new(chars);
    }

    public static implicit operator ReadOnlySpan<char>(HString? h)
    {
        return h?._string;
    }

    public static bool operator ==(HString? left, HString? right)
    {
        return string.Equals(left, right);
    }

    public static bool operator !=(HString? left, HString? right)
    {
        return !(left == right);
    }

    public static bool operator ==(HString? left, string? right)
    {
        return string.Equals(left, right);
    }

    public static bool operator !=(HString? left, string? right)
    {
        return !(left == right);
    }

    public static HString operator +(HString? left, string? right)
    {
        return new(left?._string + right);
    }

    public static HString operator +(HString? left, char right)
    {
        return new(left?._string + right);
    }

    public static HString operator +(HString? left, HString? right)
    {
        return new(left?._string + right?._string);
    }

    public static HString operator *(HString? h, int count)
    {
        if (h is null)
        {
            return Empty;
        }

        switch (count)
        {
            case <= 0:
            {
                return Empty;
            }
            case 1:
            {
                return h;
            }
            case 2:
            {
                return new(h + h);
            }
            default:
            {
                Span<char> result = stackalloc char[h.Length * count];
                ReadOnlySpan<char> span = h._string;
                for (int i = 0; i < count; i++)
                {
                    for (int j = i * h.Length; j < span.Length; j++)
                    {
                        result[j] = span[j];
                    }
                }

                return new(result);
            }
        }
    }

    public static HString operator ++(HString h)
    {
        Span<char> span = h._string.AsSpan();
        for (int i = 0; i < h.Length; i++)
        {
            span[i]++;
        }

        return h;
    }

    public static HString operator --(HString h)
    {
        Span<char> span = h._string.AsSpan();
        for (int i = 0; i < h.Length; i++)
        {
            span[i]--;
        }

        return h;
    }

    public override bool Equals(object? obj) =>
        obj switch
        {
            HString h => string.Equals(h._string, _string),
            string s => string.Equals(s, _string),
            _ => false
        };

    public override int GetHashCode() => _string.GetHashCode();

    public override string ToString() => _string;

    public object Clone() => new HString(this);

    public IEnumerator<char> GetEnumerator() => _string.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TypeCode GetTypeCode() => TypeCode.String;

    public bool ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(_string, provider);

    public byte ToByte(IFormatProvider? provider) => Convert.ToByte(_string, provider);

    public char ToChar(IFormatProvider? provider) => Convert.ToChar(_string, provider);

    public DateTime ToDateTime(IFormatProvider? provider) => Convert.ToDateTime(_string, provider);

    public decimal ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(_string, provider);

    public double ToDouble(IFormatProvider? provider) => Convert.ToDouble(_string, provider);

    public short ToInt16(IFormatProvider? provider) => Convert.ToInt16(_string, provider);

    public int ToInt32(IFormatProvider? provider) => Convert.ToInt32(_string, provider);

    public long ToInt64(IFormatProvider? provider) => Convert.ToInt64(_string, provider);

    public sbyte ToSByte(IFormatProvider? provider) => Convert.ToSByte(_string, provider);

    public float ToSingle(IFormatProvider? provider) => Convert.ToSingle(_string, provider);

    public string ToString(IFormatProvider? provider) => _string;

    public object ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(_string, conversionType, provider);

    public ushort ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(_string, provider);

    public uint ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(_string, provider);

    public ulong ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(_string, provider);
}
