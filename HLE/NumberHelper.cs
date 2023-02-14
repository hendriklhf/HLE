﻿using System;
using System.Runtime.CompilerServices;

namespace HLE;

public static class NumberHelper
{
    public static byte GetNumberLength(byte number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(number) + 1);
    }

    public static byte[] GetDigits(byte number)
    {
        Span<byte> digits = stackalloc byte[3];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(byte number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        int digitCount = 0;
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            digitCount++;
            number /= 10;
        }

        digits[^digitCount..].CopyTo(digits);
        return digitCount;
    }

    public static byte GetNumberLength(sbyte number)
    {
        byte length = (byte)(number == 0 ? 1 : (byte)Math.Floor(Math.Log10(Math.Abs(number)) + 1));
        return number < 0 ? ++length : length;
    }

    public static byte[] GetDigits(sbyte number)
    {
        Span<byte> digits = stackalloc byte[4];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(sbyte number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        number = Math.Abs(number);
        int digitCount = 0;
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            digitCount++;
            number /= 10;
        }

        digits[^digitCount..].CopyTo(digits);
        return digitCount;
    }

    public static string InsertKDots(short number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        bool isNegative = number < 0;
        number = Math.Abs(number);
        Span<char> chars = stackalloc char[numberLength];
        number.TryFormat(chars, out int length);
        chars = chars[..length];

        int dotCount = numberLength % 3 == 0 ? (numberLength / 3) - 1 : numberLength / 3;
        int total = isNegative ? numberLength + dotCount + 1 : numberLength + dotCount;
        Span<char> result = stackalloc char[total];
        int start = isNegative ? (numberLength % 3) + 1 : numberLength % 3;
        if (start == (isNegative ? 1 : 0))
        {
            start += 3;
        }

        int nextDot = start;
        for (int i = isNegative ? 1 : 0; i < total; i++)
        {
            if (i == nextDot)
            {
                result[i] = kchar;
                nextDot += 4;
            }
            else
            {
                result[i] = chars[isNegative ? i - 1 - ((nextDot - start) >> 2) : i - ((nextDot - start) >> 2)];
            }
        }

        if (isNegative)
        {
            result[0] = '-';
        }

        return new(result);
    }

    public static byte GetNumberLength(short number)
    {
        const byte one = 1;
        byte length = number == 0 ? one : (byte)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
        return number < 0 ? ++length : length;
    }

    public static byte[] GetDigits(short number)
    {
        Span<byte> digits = stackalloc byte[5];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(short number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        number = Math.Abs(number);
        int digitCount = 0;
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            digitCount++;
            number /= 10;
        }

        digits[^digitCount..].CopyTo(digits);
        return digitCount;
    }

    public static string InsertKDots(ushort number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        Span<char> chars = stackalloc char[numberLength];
        number.TryFormat(chars, out int length);
        chars = chars[..length];

        int dotCount = numberLength % 3 == 0 ? (numberLength / 3) - 1 : numberLength / 3;
        int total = numberLength + dotCount;
        Span<char> result = stackalloc char[total];
        int start = numberLength % 3;
        if (start == 0)
        {
            start += 3;
        }

        int nextDot = start;
        for (int i = 0; i < total; i++)
        {
            if (i == nextDot)
            {
                result[i] = kchar;
                nextDot += 4;
            }
            else
            {
                result[i] = chars[i - ((nextDot - start) >> 2)];
            }
        }

        return new(result);
    }

    public static byte GetNumberLength(ushort number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(number) + 1);
    }

    public static byte[] GetDigits(ushort number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(ushort number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.Length;
    }

    public static string InsertKDots(int number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        bool isNegative = number < 0;
        number = Math.Abs(number);
        Span<char> chars = stackalloc char[numberLength];
        number.TryFormat(chars, out int length);
        chars = chars[..length];

        int dotCount = numberLength % 3 == 0 ? (numberLength / 3) - 1 : numberLength / 3;
        int total = isNegative ? numberLength + dotCount + 1 : numberLength + dotCount;
        Span<char> result = stackalloc char[total];
        int start = isNegative ? (numberLength % 3) + 1 : numberLength % 3;
        if (start == (isNegative ? 1 : 0))
        {
            start += 3;
        }

        int nextDot = start;
        for (int i = isNegative ? 1 : 0; i < total; i++)
        {
            if (i == nextDot)
            {
                result[i] = kchar;
                nextDot += 4;
            }
            else
            {
                result[i] = chars[isNegative ? i - 1 - ((nextDot - start) >> 2) : i - ((nextDot - start) >> 2)];
            }
        }

        if (isNegative)
        {
            result[0] = '-';
        }

        return new(result);
    }

    public static byte GetNumberLength(int number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
    }

    public static byte[] GetDigits(int number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(int number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        number = Math.Abs(number);
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.Length;
    }

    public static string InsertKDots(uint number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        Span<char> chars = stackalloc char[numberLength];
        number.TryFormat(chars, out int length);
        chars = chars[..length];

        int dotCount = numberLength % 3 == 0 ? (numberLength / 3) - 1 : numberLength / 3;
        int total = numberLength + dotCount;
        Span<char> result = stackalloc char[total];
        int start = numberLength % 3;
        if (start == 0)
        {
            start += 3;
        }

        int nextDot = start;
        for (int i = 0; i < total; i++)
        {
            if (i == nextDot)
            {
                result[i] = kchar;
                nextDot += 4;
            }
            else
            {
                result[i] = chars[i - ((nextDot - start) >> 2)];
            }
        }

        return new(result);
    }

    public static byte GetNumberLength(uint number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(number) + 1);
    }

    public static byte[] GetDigits(uint number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(uint number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.Length;
    }

    public static string InsertKDots(long number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        bool isNegative = number < 0;
        byte isNegativeAsByte = Unsafe.As<bool, byte>(ref isNegative);
        number = isNegative ? -number : number;
        Span<char> numberChars = stackalloc char[numberLength];
        number.TryFormat(numberChars, out int length);
        numberChars = numberChars[..length];

        const byte amountOfNumbersGroupedByDots = 3;
        bool isLengthDivisibleBy3 = numberLength % amountOfNumbersGroupedByDots == 0;
        byte isLengthDivisibleBy3AsByte = Unsafe.As<bool, byte>(ref isLengthDivisibleBy3);

        int countOfDotsInNumber = (numberLength / amountOfNumbersGroupedByDots) - isLengthDivisibleBy3AsByte;
        int totalLengthOfResult = numberLength + countOfDotsInNumber + isNegativeAsByte;
        Span<char> result = stackalloc char[totalLengthOfResult];

        int startIndexInSpan = (numberLength % amountOfNumbersGroupedByDots) + isNegativeAsByte;
        startIndexInSpan += amountOfNumbersGroupedByDots * isNegativeAsByte;
        int indexOfTheNextDotInSpan = startIndexInSpan;
        for (int i = isNegativeAsByte; i < totalLengthOfResult; i++)
        {
            if (i == indexOfTheNextDotInSpan)
            {
                result[i] = kchar;
                indexOfTheNextDotInSpan += amountOfNumbersGroupedByDots + 1;
            }
            else
            {
                result[i] = numberChars[i - isNegativeAsByte - ((indexOfTheNextDotInSpan - startIndexInSpan) >> 2)];
            }
        }

        if (isNegative)
        {
            result[0] = '-';
        }

        return new(result);
    }

    public static byte GetNumberLength(long number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
    }

    public static byte[] GetDigits(long number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(long number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        number = Math.Abs(number);
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.Length;
    }

    public static string InsertKDots(ulong number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        Span<char> chars = stackalloc char[numberLength];
        number.TryFormat(chars, out int length);
        chars = chars[..length];

        int dotCount = numberLength % 3 == 0 ? (numberLength / 3) - 1 : numberLength / 3;
        int total = numberLength + dotCount;
        Span<char> result = stackalloc char[total];
        int start = numberLength % 3;
        if (start == 0)
        {
            start += 3;
        }

        int nextDot = start;
        for (int i = 0; i < total; i++)
        {
            if (i == nextDot)
            {
                result[i] = kchar;
                nextDot += 4;
            }
            else
            {
                result[i] = chars[i - ((nextDot - start) >> 2)];
            }
        }

        return new(result);
    }

    public static byte GetNumberLength(ulong number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(number) + 1);
    }

    public static byte[] GetDigits(ulong number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = GetDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int GetDigits(ulong number, Span<byte> digits)
    {
        if (number == 0)
        {
            digits[0] = 0;
            return 1;
        }

        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.Length;
    }

    public static char DigitToChar(byte digit)
    {
        if (digit > 9)
        {
            throw new InvalidOperationException("The digit must be a single char value, so can't be larger than 9.");
        }

        return (char)(digit + (byte)'0');
    }

    public static byte CharToDigit(char c)
    {
        if (c is < '0' or > '9')
        {
            throw new InvalidOperationException("The char has to be a digit.");
        }

        return (byte)(c - '0');
    }

    public static long ParsePositiveInt64(ReadOnlySpan<char> number)
    {
        long result = 0;
        for (int i = 0; i < number.Length; i++)
        {
            result = 10 * result + number[i] - '0';
        }

        return result;
    }

    public static int ParsePositiveInt32(ReadOnlySpan<char> number)
    {
        int result = 0;
        for (int i = 0; i < number.Length; i++)
        {
            result = 10 * result + number[i] - '0';
        }

        return result;
    }
}
