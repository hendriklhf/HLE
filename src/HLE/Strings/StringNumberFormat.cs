using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Strings;

public readonly struct StringNumberFormat : IEquatable<StringNumberFormat>
{
    public ReadOnlySpan<char> Chars => _chars;

    internal readonly string _chars;

    public static StringNumberFormat Binary { get; } = Create("01");

    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "not the type name")]
    public static StringNumberFormat Decimal { get; } = Create(StringConstants.Numbers);

    public static StringNumberFormat HexadecimalUpperCase { get; } = Create("0123456789ABCDEF");

    public static StringNumberFormat HexadecimalLowerCase { get; } = Create("0123456789abcdef");

    public static StringNumberFormat AlphabetUpperCase { get; } = Create(StringConstants.AlphabetUpperCase);

    public static StringNumberFormat AlphabetLowerCase { get; } = Create(StringConstants.AlphabetLowerCase);

    public static StringNumberFormat AlphaNumeric { get; } = Create(StringConstants.AlphaNumerics);

    private StringNumberFormat(string chars) => _chars = chars;

    [SkipLocalsInit]
    public static StringNumberFormat Create(char minimumCharValue, char maximumCharValue)
    {
        int charLength = maximumCharValue - minimumCharValue + 1;
        string format = StringMarshal.FastAllocateString(charLength, out Span<char> chars);
        SpanHelpers.FillAscending(chars, minimumCharValue);
        return new(format);
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
                ThrowCharIsContainedMultipleTimes(c);
            }
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCharIsContainedMultipleTimes(char c)
        => throw new InvalidOperationException($"The provided chars contain char '{c}' multiple times.");

    public bool Equals(StringNumberFormat other) => Chars.SequenceEqual(other._chars);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is StringNumberFormat other && Equals(other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(StringNumberFormat left, StringNumberFormat right) => left.Equals(right);

    public static bool operator !=(StringNumberFormat left, StringNumberFormat right) => !(left == right);
}
