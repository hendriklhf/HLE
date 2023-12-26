using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using HLE.Memory;

namespace HLE.Strings;

[DebuggerDisplay("{ToString()}")]
public readonly struct Utf8Regex(Regex regex) : IEquatable<Utf8Regex>
{
    private readonly Regex _regex = regex;

    [Pure]
    public bool IsMatch(ReadOnlySpan<byte> bytes)
    {
        using RentedArray<char> chars = ArrayPool<char>.Shared.RentAsRentedArray(bytes.Length);
        int charCount = Encoding.UTF8.GetChars(bytes, chars.AsSpan());
        return _regex.IsMatch(chars[..charCount]);
    }

    [Pure]
    public int Count(ReadOnlySpan<byte> bytes)
    {
        using RentedArray<char> chars = ArrayPool<char>.Shared.RentAsRentedArray(bytes.Length);
        int charCount = Encoding.UTF8.GetChars(bytes, chars.AsSpan());
        return _regex.Count(chars[..charCount]);
    }

    public override string ToString() => _regex.ToString();

    public bool Equals(Utf8Regex other) => _regex == other._regex;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Utf8Regex other && Equals(other);

    public override int GetHashCode() => _regex.GetHashCode();

    public static bool operator ==(Utf8Regex left, Utf8Regex right) => left.Equals(right);

    public static bool operator !=(Utf8Regex left, Utf8Regex right) => !(left == right);
}
