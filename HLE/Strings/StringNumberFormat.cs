using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Strings;

public readonly struct StringNumberFormat : IEquatable<StringNumberFormat>
{
    public ReadOnlySpan<char> Chars => _chars;

    internal readonly string _chars;

    public static StringNumberFormat Binary { get; } = Create("01");

    public static StringNumberFormat Decimal { get; } = Create(StringConstants.Numbers);

    public static StringNumberFormat HexadecimalUpperCase { get; } = Create("0123456789ABCDEF");

    public static StringNumberFormat HexadecimalLowerCase { get; } = Create("0123456789abcdef");

    public static StringNumberFormat AlphabetUpperCase { get; } = Create(StringConstants.AlphabetUpperCase);

    public static StringNumberFormat AlphabetLowerCase { get; } = Create(StringConstants.AlphabetLowerCase);

    public static StringNumberFormat AlphaNumeric { get; } = Create(StringConstants.AlphaNumerics);

    private StringNumberFormat(string chars)
    {
        _chars = chars;
    }

    [SkipLocalsInit]
    public static StringNumberFormat Create(char minimumCharValue, char maximumCharValue)
    {
        int charLength = maximumCharValue - minimumCharValue + 1;
        Span<char> chars;
        if (!MemoryHelper.UseStackAlloc<char>(charLength))
        {
            using RentedArray<char> rentedBuffer = new(charLength);
            chars = rentedBuffer.AsSpan()[..charLength];
            MemoryMarshal.Cast<char, ushort>(chars).FillAscending(minimumCharValue);
            return new(StringPool.Shared.GetOrAdd(chars));
        }

        chars = SpanMarshal.ReturnStackAlloced(stackalloc char[charLength]);
        MemoryMarshal.Cast<char, ushort>(chars).FillAscending(minimumCharValue);
        return new(StringPool.Shared.GetOrAdd(chars));
    }

    public static StringNumberFormat Create(ReadOnlySpan<char> chars)
    {
        ValidateChars(chars);
        return new(StringPool.Shared.GetOrAdd(chars));
    }

    public static StringNumberFormat Create(string chars)
    {
        ValidateChars(chars);
        return new(StringPool.Shared.GetOrAdd(chars));
    }

    private static void ValidateChars(ReadOnlySpan<char> chars)
    {
        foreach (char c in chars)
        {
            if (chars.Count(c) != 1)
            {
                throw new InvalidOperationException($"The provided chars contain char '{c}' multiple times.");
            }
        }
    }

    public bool Equals(StringNumberFormat other)
    {
        return Chars.SequenceEqual(other._chars);
    }

    public override bool Equals(object? obj)
    {
        return obj is StringNumberFormat other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(StringNumberFormat left, StringNumberFormat right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StringNumberFormat left, StringNumberFormat right)
    {
        return !(left == right);
    }
}
