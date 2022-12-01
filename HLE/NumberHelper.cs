using System;

namespace HLE;

public static class NumberHelper
{
#pragma warning disable IDE0060
    public static string InsertKDots(byte number, char kchar = '.')
    {
        return number.ToString();
    }
#pragma warning restore IDE0060

    public static byte GetNumberLength(byte number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(number) + 1);
    }

    public static char[] NumberToChars(byte number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[..length].ToArray();
    }

    public static int NumberToChars(byte number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(byte number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(byte number, Span<byte> digits)
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

#pragma warning disable IDE0060
    public static string InsertKDots(sbyte number, char kchar = '.')
    {
        return number.ToString();
    }
#pragma warning restore IDE0060

    public static byte GetNumberLength(sbyte number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
    }

    public static char[] NumberToChars(sbyte number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[length..].ToArray();
    }

    public static int NumberToChars(sbyte number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        number = Math.Abs(number);
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(sbyte number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(sbyte number, Span<byte> digits)
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
        int charsLength = NumberToChars(number, chars);
        chars = chars[..charsLength];

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
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
    }

    public static char[] NumberToChars(short number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[length..].ToArray();
    }

    public static int NumberToChars(short number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        number = Math.Abs(number);
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(short number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(short number, Span<byte> digits)
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

    public static string InsertKDots(ushort number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        Span<char> chars = stackalloc char[numberLength];
        int charsLength = NumberToChars(number, chars);
        chars = chars[..charsLength];

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

    public static char[] NumberToChars(ushort number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[..length].ToArray();
    }

    public static int NumberToChars(ushort number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(ushort number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(ushort number, Span<byte> digits)
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
        int charsLength = NumberToChars(number, chars);
        chars = chars[..charsLength];

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

    public static char[] NumberToChars(int number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[..length].ToArray();
    }

    public static int NumberToChars(int number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        number = Math.Abs(number);
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(int number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(int number, Span<byte> digits)
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
        int charsLength = NumberToChars(number, chars);
        chars = chars[..charsLength];

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

    public static char[] NumberToChars(uint number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[..length].ToArray();
    }

    public static int NumberToChars(uint number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(uint number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(uint number, Span<byte> digits)
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
        number = Math.Abs(number);
        Span<char> chars = stackalloc char[numberLength];
        int charsLength = NumberToChars(number, chars);
        chars = chars[..charsLength];

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

    public static byte GetNumberLength(long number)
    {
        const byte one = 1;
        return number == 0 ? one : (byte)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
    }

    public static char[] NumberToChars(long number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[length..].ToArray();
    }

    public static int NumberToChars(long number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        number = Math.Abs(number);
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(long number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(long number, Span<byte> digits)
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
        int charsLength = NumberToChars(number, chars);
        chars = chars[..charsLength];

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

    public static char[] NumberToChars(ulong number)
    {
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        int length = NumberToChars(number, chars);
        return chars[..length].ToArray();
    }

    public static int NumberToChars(ulong number, Span<char> chars)
    {
        if (number == 0)
        {
            chars[0] = '0';
            return 1;
        }

        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.Length;
    }

    public static byte[] NumberToDigits(ulong number)
    {
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        int length = NumberToDigits(number, digits);
        return digits[..length].ToArray();
    }

    public static int NumberToDigits(ulong number, Span<byte> digits)
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
}
