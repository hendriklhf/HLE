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

    public static char[] NumberToCharArray(byte number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(byte number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
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

    public static char[] NumberToCharArray(sbyte number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        number = Math.Abs(number);
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(sbyte number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        number = Math.Abs(number);
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
    }

    public static string InsertKDots(short number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        bool isNegative = number < 0;
        // int to char span, copied from ToCharArray in order to keep the chars on the stack
        number = Math.Abs(number);
        Span<char> chars = stackalloc char[numberLength];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }
        // end of copied code

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

    public static char[] NumberToCharArray(short number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        number = Math.Abs(number);
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(short number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        number = Math.Abs(number);
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
    }

    public static string InsertKDots(ushort number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        // int to char span, copied from ToCharArray in order to keep the chars on the stack
        Span<char> chars = stackalloc char[numberLength];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }
        // end of copied code

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

    public static char[] NumberToCharArray(ushort number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(ushort number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
    }

    public static string InsertKDots(int number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        bool isNegative = number < 0;
        // int to char span, copied from ToCharArray in order to keep the chars on the stack
        number = Math.Abs(number);
        Span<char> chars = stackalloc char[numberLength];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }
        // end of copied code

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

    public static char[] NumberToCharArray(int number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        number = Math.Abs(number);
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(int number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        number = Math.Abs(number);
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
    }

    public static string InsertKDots(uint number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        // int to char span, copied from ToCharArray in order to keep the chars on the stack
        Span<char> chars = stackalloc char[numberLength];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }
        // end of copied code

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

    public static char[] NumberToCharArray(uint number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(uint number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
    }

    public static string InsertKDots(long number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        bool isNegative = number < 0;
        // int to char span, copied from ToCharArray in order to keep the chars on the stack
        number = Math.Abs(number);
        Span<char> chars = stackalloc char[numberLength];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }
        // end of copied code

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

    public static char[] NumberToCharArray(long number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        number = Math.Abs(number);
        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(long number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        number = Math.Abs(number);
        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
    }

    public static string InsertKDots(ulong number, char kchar = '.')
    {
        int numberLength = GetNumberLength(number);
        if (numberLength < 4)
        {
            return number.ToString();
        }

        // int to char span, copied from ToCharArray in order to keep the chars on the stack
        Span<char> chars = stackalloc char[numberLength];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }
        // end of copied code

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

    public static char[] NumberToCharArray(ulong number)
    {
        if (number == 0)
        {
            return new[]
            {
                '0'
            };
        }

        Span<char> chars = stackalloc char[GetNumberLength(number)];
        for (int i = chars.Length - 1; number > 0; i--)
        {
            chars[i] = DigitToChar((byte)(number % 10));
            number /= 10;
        }

        return chars.ToArray();
    }

    public static byte[] NumberToDigitArray(ulong number)
    {
        if (number == 0)
        {
            return new byte[]
            {
                0
            };
        }

        Span<byte> digits = stackalloc byte[GetNumberLength(number)];
        for (int i = digits.Length - 1; number > 0; i--)
        {
            digits[i] = (byte)(number % 10);
            number /= 10;
        }

        return digits.ToArray();
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
