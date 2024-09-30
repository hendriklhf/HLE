using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Text;

public readonly struct StringNumberFormatter(StringNumberFormat format) : IEquatable<StringNumberFormatter>
{
    public StringNumberFormat NumberFormat { get; } = format;

    [SkipLocalsInit]
    public string Format(int number)
    {
        Span<char> result = stackalloc char[64];
        ref char charsReference = ref MemoryMarshal.GetReference(NumberFormat.Chars);
        int charsLength = NumberFormat._chars.Length;

        int writeIndex = result.Length - 1;
        while (number >= charsLength)
        {
            int charIndex = number % charsLength;
            result[writeIndex--] = Unsafe.Add(ref charsReference, charIndex);
            number /= charsLength;
        }

        result[writeIndex--] = Unsafe.Add(ref charsReference, number);
        int writtenChars = result.Length - 1 - writeIndex;
        return new(result[^writtenChars..]);
    }

    public bool TryFormat(int number, Span<char> result, out int writtenChars)
    {
        ReadOnlySpan<char> chars = NumberFormat.Chars;
        int charsLength = chars.Length;

        int writeIndex = result.Length - 1;
        while (number >= charsLength)
        {
            int charIndex = number % charsLength;
            if (writeIndex < 0)
            {
                writtenChars = 0;
                return false;
            }

            result[writeIndex--] = chars[charIndex];
            number /= charsLength;
        }

        if (writeIndex < 0)
        {
            writtenChars = 0;
            return false;
        }

        result[writeIndex--] = chars[number];
        writtenChars = result.Length - 1 - writeIndex;
        result[^writtenChars..].CopyTo(result);
        return true;
    }

    [Pure]
    public int Parse(ref PooledInterpolatedStringHandler stringNumber)
    {
        int number = Parse(stringNumber.Text);
        stringNumber.Dispose();
        return number;
    }

    [Pure]
    public int Parse(string stringNumber) => Parse(stringNumber.AsSpan());

    [Pure]
    public int Parse(ReadOnlySpan<char> stringNumber)
    {
        int result = 0;
        int exponent = 0;
        ReadOnlySpan<char> chars = NumberFormat.Chars;
        ref char numberReference = ref MemoryMarshal.GetReference(stringNumber);
        for (int i = stringNumber.Length - 1; i >= 0; i--)
        {
            char c = Unsafe.Add(ref numberReference, i);
            int index = chars.IndexOf(c);
            if (index < 0)
            {
                ThrowWrongNumberFormat();
            }

            checked
            {
                result += index * (int)Math.Pow(chars.Length, exponent++);
            }
        }

        return result;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowWrongNumberFormat()
            => throw new FormatException($"The provided number is in an invalid format. It does not match the provided {typeof(StringNumberFormat)}");
    }

    public bool Equals(StringNumberFormatter other) => NumberFormat.Equals(other.NumberFormat);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is StringNumberFormatter other && Equals(other);

    public override int GetHashCode() => string.GetHashCode(NumberFormat._chars);

    public static bool operator ==(StringNumberFormatter left, StringNumberFormatter right) => left.Equals(right);

    public static bool operator !=(StringNumberFormatter left, StringNumberFormatter right) => !(left == right);
}
