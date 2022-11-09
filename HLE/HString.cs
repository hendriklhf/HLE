using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HLE;

[DebuggerDisplay("\"{GetString()}\" Length = {Length}")]
public sealed class HString : IEnumerable<char>, ICloneable, IConvertible
{
    public char this[int idx]
    {
        get => GetChar(idx);
        set => SetChar(idx, value);
    }

    public char this[Index index]
    {
        get => GetString()[index];
        set => SetIndex(index, value);
    }

    public string this[Range range]
    {
        get => GetString()[range];
        set => SetRange(range, value);
    }

    public int Length => _chars.Length;

    public static HString Empty { get; } = new(string.Empty);

    private char[] _chars;
    private string? _string;

    public HString(IEnumerable<char>? chars)
    {
        _chars = chars switch
        {
            string s => s.ToCharArray(),
            HString h => (char[])h._chars.Clone(),
            char[] c => c,
            not null => chars.ToArray(),
            _ => Array.Empty<char>()
        };
    }

    public HString Replace(char oldValue, char newValue) => GetString().Replace(oldValue, newValue);

    public HString Replace(char oldValue, string newValue) => GetString().Replace(oldValue.ToString(), newValue);

    public HString Replace(string oldValue, char newValue) => GetString().Replace(oldValue, newValue.ToString());

    public HString Replace(string oldValue, string newValue) => GetString().Replace(oldValue, newValue);

    public HString Insert(int startIdx, string value) => GetString().Insert(startIdx, value);

    public bool Contains(char value) => _chars.Contains(value);

    public bool Contains(string value) => GetString().Contains(value);

    public HString[] Split(char separator = ' ') => GetString().Split(separator).Select(s => new HString(s)).ToArray();

    public HString[] Split(string separator) => GetString().Split(separator).Select(s => new HString(s)).ToArray();

    public HString[] Split(int charCount, bool onlySplitOnWhitespace = false) => GetString().Split(charCount, onlySplitOnWhitespace).Select(s => new HString(s)).ToArray();

    public HString Trim() => GetString().Trim();

    public HString TrimAll() => GetString().TrimAll();

    public bool StartsWith(char value) => GetString().StartsWith(value);

    public bool StartsWith(string value) => GetString().StartsWith(value, StringComparison.CurrentCulture);

    public bool EndsWith(char value) => GetString().EndsWith(value);

    public bool EndsWith(string value) => GetString().EndsWith(value);

    public HString ToLower() => GetString().ToLower();

    public HString ToUpper() => GetString().ToUpper();

    public char[] ToCharArray() => (char[])_chars.Clone();

    private void SetChar(int idx, char c)
    {
        if (idx < 0)
        {
            throw new IndexOutOfRangeException($"Array index {idx} can't be negative.");
        }

        if (idx >= _chars.Length)
        {
            Array.Resize(ref _chars, idx + 1);
        }

        _chars[idx] = c;
        _string = null;
    }

    private char GetChar(int idx)
    {
        if (idx < 0 || idx >= _chars.Length)
        {
            throw new IndexOutOfRangeException($"Out of range for index {idx}. Array has only a length of {_chars.Length}.");
        }

        return _chars[idx];
    }

    private string GetString()
    {
        return _string ??= string.Concat(_chars);
    }

    public void SetIndex(Index index, char value)
    {
        int idx = index.IsFromEnd ? _chars.Length - index.Value - 1 : index.Value;
        this[idx] = value;
        _string = null;
    }

    private void SetRange(Range range, string value)
    {
        int start = range.Start.Value;
        int end = range.End.IsFromEnd ? _chars.Length - range.End.Value - 1 : range.End.Value;

        if (start > end)
        {
            throw new InvalidOperationException("The starting index can't be larger than the ending index.");
        }

        int rangeLength = end - start + 1;
        if (rangeLength != value.Length)
        {
            throw new InvalidOperationException($"Parameter {nameof(range)} and {nameof(value)} need to have the same length. Length of {nameof(range)} is {rangeLength} and length of {nameof(value)} is {value.Length}");
        }

        for (int i = start; i <= end; i++)
        {
            this[i] = value[i - start];
        }

        _string = null;
    }

    public static implicit operator string(HString? h)
    {
        return h?.GetString() ?? string.Empty;
    }

    public static implicit operator HString(string? str)
    {
        return new(str);
    }

    public static implicit operator HString(char c)
    {
        return new(new[]
        {
            c
        });
    }

    public static implicit operator HString(char[]? chars)
    {
        return new(chars);
    }

    public static bool operator ==(HString left, HString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HString left, HString right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(HString left, string right)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        return left.Equals(right);
    }

    public static bool operator !=(HString left, string right)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        return !left.Equals(right);
    }

    public static HString operator +(HString left, string right)
    {
        return new(left.GetString() + right);
    }

    public static HString operator +(HString left, char right)
    {
        return new(left.GetString() + right);
    }

    public static HString operator +(HString left, HString right)
    {
        return new(left.GetString() + right.GetString());
    }

    public static HString operator *(HString h, int count)
    {
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
                StringBuilder builder = new();
                string value = h.GetString();
                for (int i = 0; i < count; i++)
                {
                    builder.Append(value);
                }

                return new(builder.ToString());
            }
        }
    }

    public static HString operator ++(HString h)
    {
        for (int i = 0; i < h.Length; i++)
        {
            h._chars[i]++;
        }

        h._string = null;
        return h;
    }

    public static HString operator --(HString h)
    {
        for (int i = 0; i < h.Length; i++)
        {
            h._chars[i]--;
        }

        h._string = null;
        return h;
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            HString h => GetString() == h.GetString(),
            string s => GetString() == s,
            char c => GetString().Length == 1 && GetString()[0] == c,
            _ => false
        };
    }

    public override int GetHashCode() => GetString().GetHashCode();

    public override string ToString() => GetString();

    public object Clone() => new HString(this);

    public IEnumerator<char> GetEnumerator() => _chars.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TypeCode GetTypeCode() => TypeCode.String;

    public bool ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(GetString(), provider);

    public byte ToByte(IFormatProvider? provider) => Convert.ToByte(GetString(), provider);

    public char ToChar(IFormatProvider? provider) => Convert.ToChar(GetString(), provider);

    public DateTime ToDateTime(IFormatProvider? provider) => Convert.ToDateTime(GetString(), provider);

    public decimal ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(GetString(), provider);

    public double ToDouble(IFormatProvider? provider) => Convert.ToDouble(GetString(), provider);

    public short ToInt16(IFormatProvider? provider) => Convert.ToInt16(GetString(), provider);

    public int ToInt32(IFormatProvider? provider) => Convert.ToInt32(GetString(), provider);

    public long ToInt64(IFormatProvider? provider) => Convert.ToInt64(GetString(), provider);

    public sbyte ToSByte(IFormatProvider? provider) => Convert.ToSByte(GetString(), provider);

    public float ToSingle(IFormatProvider? provider) => Convert.ToSingle(GetString(), provider);

    public string ToString(IFormatProvider? provider) => GetString();

    public object ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(GetString(), conversionType, provider);

    public ushort ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(GetString(), provider);

    public uint ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(GetString(), provider);

    public ulong ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(GetString(), provider);
}
