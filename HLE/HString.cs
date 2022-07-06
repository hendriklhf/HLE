using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLE.Collections;

namespace HLE;

public class HString : IEnumerable<char>, ICloneable
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

    public int[] IndecesOf(char value) => _chars.IndecesOf(c => c == value).ToArray();

    public bool StartsWith(char value) => GetString().StartsWith(value);

    public bool StartsWith(string value) => GetString().StartsWith(value);

    public bool EndsWith(char value) => GetString().EndsWith(value);

    public bool EndsWith(string value) => GetString().EndsWith(value);

    public string ToLower() => GetString().ToLower();

    public string ToUpper() => GetString().ToUpper();

    public char[] ToCharArray() => _chars;

    public char RandomChar() => _chars.Random();

    private void SetChar(int idx, char c)
    {
        if (idx < 0)
        {
            throw new IndexOutOfRangeException();
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
            throw new IndexOutOfRangeException();
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
            case 1:
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

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            HString h => GetString() == h.GetString(),
            string s => GetString() == s,
            char c => GetString().Length == 1 && GetString().First() == c,
            _ => false
        };
    }

    public object Clone()
    {
        return new HString((char[])_chars.Clone());
    }

    public IEnumerator<char> GetEnumerator()
    {
        return _chars.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
