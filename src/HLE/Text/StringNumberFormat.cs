using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Text;

public readonly struct StringNumberFormat : IEquatable<StringNumberFormat>
{
    public ReadOnlySpan<char> Chars => _chars;

    internal readonly string _chars;

    public static StringNumberFormat Binary { get; } = Create("01");

    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "not the type's name")]
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
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumCharValue, maximumCharValue);
        int charLength = maximumCharValue - minimumCharValue + 1;
        string format = StringMarshal.FastAllocateString(charLength, out Span<char> chars);
        SpanHelpers.FillAscending(chars, minimumCharValue);
        return new(format);
    }

    [Pure]
    public static StringNumberFormat Create(ref PooledInterpolatedStringHandler chars)
    {
        StringNumberFormat format = Create(chars.Text);
        chars.Dispose();
        return format;
    }

    [Pure]
    public static StringNumberFormat Create(ReadOnlySpan<char> chars)
    {
        ValidateChars(chars);
        return new(StringPool.Shared.GetOrAdd(chars));
    }

    [Pure]
    public static StringNumberFormat Create(string chars)
    {
        ValidateChars(chars);
        return new(StringPool.Shared.GetOrAdd(chars));
    }

    private static void ValidateChars(ReadOnlySpan<char> chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (chars[i..].Count(c) != 1)
            {
                ThrowContainsCharMultipleTimes(c);
            }
        }

        return;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowContainsCharMultipleTimes(char c)
            => throw new InvalidOperationException($"The provided chars contain char '{c}' multiple times.");
    }

    public bool Equals(StringNumberFormat other) => Chars.SequenceEqual(other._chars);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is StringNumberFormat other && Equals(other);

    public override int GetHashCode() => string.GetHashCode(Chars);

    public static bool operator ==(StringNumberFormat left, StringNumberFormat right) => left.Equals(right);

    public static bool operator !=(StringNumberFormat left, StringNumberFormat right) => !(left == right);
}
