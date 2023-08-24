using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Strings;

public readonly struct StringNumberCreator : IEquatable<StringNumberCreator>
{
    public StringNumberFormat Format { get; }

    public StringNumberCreator(StringNumberFormat format)
    {
        Format = format;
    }

    [SkipLocalsInit]
    public string Create(int number)
    {
        Span<char> result = stackalloc char[64];
        ref char charsReference = ref MemoryMarshal.GetReference(Format.Chars);
        int charsLength = Format._chars.Length;

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

    public bool TryCreate(int number, Span<char> result, out int writtenChars)
    {
        ReadOnlySpan<char> chars = Format.Chars;
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

    public int Revert(string stringNumber) => Revert(stringNumber.AsSpan());

    public int Revert(ReadOnlySpan<char> stringNumber)
    {
        int result = 0;
        int exponent = 0;
        ReadOnlySpan<char> chars = Format.Chars;
        ref char numberReference = ref MemoryMarshal.GetReference(stringNumber);
        for (int i = stringNumber.Length - 1; i >= 0; i--)
        {
            char c = Unsafe.Add(ref numberReference, i);
            int index = chars.IndexOf(c);
            if (index < 0)
            {
                throw new FormatException($"The provided number is in an invalid format. It does not match the provided {typeof(StringNumberFormat)}");
            }

            checked
            {
                result += index * (int)Math.Pow(chars.Length, exponent++);
            }
        }

        return result;
    }

    public bool Equals(StringNumberCreator other)
    {
        return Format.Equals(other.Format);
    }

    public override bool Equals(object? obj)
    {
        return obj is StringNumberCreator other && Equals(other);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(Format._chars);
    }

    public static bool operator ==(StringNumberCreator left, StringNumberCreator right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StringNumberCreator left, StringNumberCreator right)
    {
        return !(left == right);
    }
}
