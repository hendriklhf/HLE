using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HLE.Marshalling;

namespace HLE.Text;

public struct StringEnumerator(string str) : IEnumerator<char>, IEquatable<StringEnumerator>
{
    public readonly ref readonly char Current => ref Unsafe.Add(ref StringMarshal.GetReference(_str), (uint)_current);

    readonly char IEnumerator<char>.Current => Current;

    readonly object IEnumerator.Current => Current;

    private readonly string _str = str;
    private int _current = -1;

    public static StringEnumerator Empty => new(string.Empty);

    public bool MoveNext() => ++_current < _str.Length;

    public void Reset() => _current = -1;

    readonly void IDisposable.Dispose()
    {
    }

    public readonly bool Equals(StringEnumerator other) => _current == other._current && other._str == _str;

    public override readonly bool Equals(object? obj) => obj is StringEnumerator other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_str, _current);

    public static bool operator ==(StringEnumerator left, StringEnumerator right) => left.Equals(right);

    public static bool operator !=(StringEnumerator left, StringEnumerator right) => !(left == right);
}
