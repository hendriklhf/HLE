using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HLE.Marshalling;

namespace HLE.Text;

public struct StringEnumerator(string str) : IEnumerator<char>, IEquatable<StringEnumerator>
{
    public readonly char Current => Unsafe.Add(ref StringMarshal.GetReference(_str), _current);

    readonly object IEnumerator.Current => Current;

    private readonly string _str = str;
    private int _current = -1;

    public bool MoveNext() => ++_current < _str.Length;

    public void Reset() => _current = -1;

    public readonly void Dispose()
    {
    }

    public readonly bool Equals(StringEnumerator other) => _current == other._current && other._str == _str;

    public override readonly bool Equals(object? obj) => obj is StringEnumerator other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_str, _current);

    public static bool operator ==(StringEnumerator left, StringEnumerator right) => left.Equals(right);

    public static bool operator !=(StringEnumerator left, StringEnumerator right) => !(left == right);
}
