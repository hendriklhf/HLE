using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Numerics;

public static class NumberHelper
{
    [Pure]
    public static string InsertThousandSeparators<T>(T number, char separator = '.') where T : INumber<T>
    {
        Span<char> resultBuffer = stackalloc char[30];
        int length = InsertThousandSeparators(number, separator, resultBuffer);
        return new(resultBuffer[..length]);
    }

    public static int InsertThousandSeparators<T>(T number, char separator, Span<char> resultBuffer) where T : INumber<T>
    {
        Span<byte> digits = stackalloc byte[30];
        int digitsLength = GetDigits(number, digits);

        bool isNegative = number < T.Zero;
        int isNegativeAsByte = Unsafe.As<bool, byte>(ref isNegative);

        bool isDivisibleBy3 = digitsLength % 3 == 0;
        int isDivisibleBy3AsByte = Unsafe.As<bool, byte>(ref isDivisibleBy3);

        int countOfDotsInNumber = (digitsLength / 3) - isDivisibleBy3AsByte;
        int totalNumberLength = digitsLength + countOfDotsInNumber;
        int resultLength = 0;
        int digitIndex = digitsLength - 1;
        int writeIndex = totalNumberLength + isNegativeAsByte - 1;
        int writtenSeparatorCount = 0;
        while (resultLength < totalNumberLength)
        {
            resultBuffer[writeIndex--] = DigitToChar(digits[digitIndex--]);
            resultLength++;

            bool needsToWriteSeparator = resultLength > 0 && resultLength + 1 < totalNumberLength && (resultLength - writtenSeparatorCount) % 3 == 0;
            if (!needsToWriteSeparator)
            {
                continue;
            }

            resultBuffer[writeIndex--] = separator;
            writtenSeparatorCount++;
            resultLength++;
        }

        if (isNegative)
        {
            resultBuffer[0] = '-';
        }

        return resultLength + isNegativeAsByte;
    }

    [Pure]
    public static int GetNumberLength<T>(T number) where T : INumber<T>
    {
        return number == T.Zero ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(double.CreateTruncating(number))) + 1);
    }

    [Pure]
    public static byte[] GetDigits<T>(T number) where T : INumber<T>
    {
        Span<byte> digits = stackalloc byte[30];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits<T>(T number, Span<byte> digits) where T : INumber<T>
    {
        if (number == T.Zero)
        {
            digits[0] = 0;
            return 1;
        }

        if (number < T.Zero)
        {
            number = T.Abs(number);
        }

        int writtenDigits = 0;
        T ten = T.CreateTruncating(10);
        for (int i = digits.Length - 1; number > T.Zero; i--)
        {
            digits[i] = byte.CreateTruncating(number % ten);
            writtenDigits++;
            number /= ten;
        }

        digits[^writtenDigits..].CopyTo(digits);
        return writtenDigits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char DigitToChar(byte digit)
    {
        return (char)(digit + (byte)'0');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte CharToDigit(char c)
    {
        return (byte)(c - '0');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte CharToDigit(byte c)
    {
        return (byte)(c - '0');
    }

    [Pure]
    public static T ParsePositiveNumber<T>(ReadOnlySpan<char> number) where T : INumberBase<T>
    {
        T result = T.Zero;
        T ten = T.CreateTruncating(10);
        T charZero = T.CreateTruncating('0');
        for (int i = 0; i < number.Length; i++)
        {
            result = ten * result + T.CreateTruncating(number[i]) - charZero;
        }

        return result;
    }

    [Pure]
    public static T ParsePositiveNumber<T>(ReadOnlySpan<byte> number) where T : INumberBase<T>
    {
        T result = T.Zero;
        T ten = T.CreateTruncating(10);
        T charZero = T.CreateTruncating((int)'0');
        for (int i = 0; i < number.Length; i++)
        {
            result = ten * result + T.CreateTruncating(number[i]) - charZero;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T SetSignBitToZero<T>(T number) where T : ISignedNumber<T>, IBitwiseOperators<T, T, T>
    {
        return sizeof(T) switch
        {
            sizeof(long) => T.CreateTruncating(number) & T.CreateTruncating(0x7FFFFFFFFFFFFFFF),
            sizeof(int) => T.CreateTruncating(number) & T.CreateTruncating(0x7FFFFFFF),
            sizeof(short) => T.CreateTruncating(number) & T.CreateTruncating(0x7FFF),
            sizeof(sbyte) => T.CreateTruncating(number) & T.CreateTruncating(0x7F),
            _ => throw new UnreachableException("This shouldn't happen, as all number types are covered.")
        };
    }
}
