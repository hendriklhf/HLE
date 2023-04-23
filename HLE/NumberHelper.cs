using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE;

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
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            bool success = number.TryFormat(resultBuffer, out int charsWritten, ReadOnlySpan<char>.Empty, null);
            if (!success)
            {
                throw new ArgumentException("There was not enough space left in the buffer to write the provided value to the buffer.", nameof(resultBuffer));
            }

            return charsWritten;
        }

        bool isNegative = number < T.Zero;
        byte isNegativeAsByte = Unsafe.As<bool, byte>(ref isNegative);
        number = isNegative ? -number : number;
        Span<char> numberChars = stackalloc char[numberLength];
        number.TryFormat(numberChars, out int length, ReadOnlySpan<char>.Empty, null);
        numberChars = numberChars[..length];

        const byte amountOfNumbersGroupedBySeparator = 3;
        bool isLengthDivisibleBy3 = numberLength % amountOfNumbersGroupedBySeparator == 0;
        byte isLengthDivisibleBy3AsByte = Unsafe.As<bool, byte>(ref isLengthDivisibleBy3);

        int countOfDotsInNumber = (numberLength / amountOfNumbersGroupedBySeparator) - isLengthDivisibleBy3AsByte;
        int totalLengthOfResult = numberLength + countOfDotsInNumber + isNegativeAsByte;

        int startIndexInSpan = (numberLength % amountOfNumbersGroupedBySeparator) + isNegativeAsByte;
        startIndexInSpan += amountOfNumbersGroupedBySeparator * isNegativeAsByte;
        int indexOfTheNextDotInSpan = startIndexInSpan;
        int resultLength = 0;
        for (int i = isNegativeAsByte; i < totalLengthOfResult; i++)
        {
            if (i == indexOfTheNextDotInSpan)
            {
                resultBuffer[resultLength++] = separator;
                indexOfTheNextDotInSpan += amountOfNumbersGroupedBySeparator + 1;
            }
            else
            {
                resultBuffer[resultLength++] = numberChars[i - isNegativeAsByte - ((indexOfTheNextDotInSpan - startIndexInSpan) >> 2)];
            }
        }

        if (isNegative)
        {
            resultBuffer[0] = '-';
        }

        return resultLength;
    }

    [Pure]
    public static int GetNumberLength<T>(T number) where T : INumber<T>
    {
        return number == T.Zero ? 1 : (int)Math.Floor(Math.Log10(double.CreateTruncating(number)) + 1);
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
        T result = default!;
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
        T result = default!;
        T ten = T.CreateTruncating(10);
        T charZero = T.CreateTruncating('0');
        for (int i = 0; i < number.Length; i++)
        {
            result = ten * result + T.CreateTruncating(number[i]) - charZero;
        }

        return result;
    }

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
