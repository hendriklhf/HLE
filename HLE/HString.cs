using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HLE;

[DebuggerDisplay("\"{GetString()}\"  Length = {Length}")]
public class HString : IEnumerable<char>, ICloneable, IConvertible
{
    public char this[int idx]
    {
        get => GetChar(idx);
        set => SetChar(idx, value);
    }

    public char this[Index index] => GetString()[index];

    public string this[Range r] => GetString()[r];

    public int Length => GetString().Length;

    private char[] _chars;
    private string? _string;

    private HString(IEnumerable<char> chars)
    {
        _chars = chars switch
        {
            string s => s.ToCharArray(),
            HString h => (char[])h._chars.Clone(),
            char[] c => c,
            _ => chars.ToArray()
        };
    }

    public string Replace(char oldValue, char newValue) => GetString().Replace(oldValue, newValue);

    public string Replace(char oldValue, string newValue) => GetString().Replace(oldValue.ToString(), newValue);

    public string Replace(string oldValue, char newValue) => GetString().Replace(oldValue, newValue.ToString());

    public string Replace(string oldValue, string newValue) => GetString().Replace(oldValue, newValue);

    public string Remove(char value) => GetString().Replace(value.ToString(), string.Empty);

    public string Remove(string value) => GetString().Replace(value, string.Empty);

    public string Insert(int startIdx, string value) => GetString().Insert(startIdx, value);

    public bool Contains(char value) => _chars.Contains(value);

    public bool Contains(string value) => GetString().Contains(value);

    public string[] Split(char separator = ' ') => GetString().Split(separator);

    public string[] Split(string separator) => GetString().Split(separator);

    public string[] Split(int charCount, bool onlySplitOnWhitespace = false) => GetString().Split(charCount, onlySplitOnWhitespace).ToArray();

    public string Trim() => GetString().Trim();

    public string TrimAll() => GetString().TrimAll();

    public bool StartsWith(char value) => GetString().StartsWith(value);

    public bool StartsWith(string value) => GetString().StartsWith(value);

    public bool EndsWith(char value) => GetString().EndsWith(value);

    public bool EndsWith(string value) => GetString().EndsWith(value);

    public string ToLower() => GetString().ToLower();

    public string ToUpper() => GetString().ToUpper();

    public char[] ToCharArray() => (char[])_chars.Clone();

    private void SetChar(int idx, char c)
    {
        if (idx < 0)
        {
            throw new IndexOutOfRangeException($"Index {idx} can't be negative.");
        }

        if (idx >= _chars.Length)
        {
            char[] arr = new char[idx + 1];
            Array.Copy(_chars, arr, _chars.Length);
            _chars = arr;
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
        if (_string is not null)
        {
            return _string;
        }

        _string = string.Concat(_chars);
        return _string;
    }

    public static implicit operator string(HString hString)
    {
        return hString.GetString();
    }

    public static implicit operator HString(string str)
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

    public static implicit operator HString(char[] chars)
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

    public static bool operator ==(HString left, char right)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        return left.Equals(right);
    }

    public static bool operator !=(HString left, char right)
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

    public static HString operator *(HString left, int count)
    {
        switch (count)
        {
            case <= 1:
            {
                return left;
            }
            case 2:
            {
                return new HString(left + left);
            }
            default:
            {
                StringBuilder builder = new();
                string value = left.GetString();
                for (int i = 0; i < count; i++)
                {
                    builder.Append(value);
                }

                return new HString(builder.ToString());
            }
        }
    }

    public static HString operator ++(HString left)
    {
        for (int i = 0; i < left.Length; i++)
        {
            left._chars[i]++;
        }

        left._string = null;
        return left;
    }

    public static HString operator --(HString left)
    {
        for (int i = 0; i < left.Length; i++)
        {
            left._chars[i]--;
        }

        left._string = null;
        return left;
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

    public object Clone() => new HString((char[])_chars.Clone());

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
