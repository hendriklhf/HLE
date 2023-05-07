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
    public static unsafe T SetSignBitToZero<T>(T number) where T : IBinaryInteger<T>, ISignedNumber<T>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOnlyOneBitSet<T>(T number) where T : INumberBase<T>, IBitwiseOperators<T, T, T>
    {
        return (number & (number - T.One)) == T.Zero && number != T.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOnlyOneBitOrZeroSet<T>(T number) where T : INumberBase<T>, IBitwiseOperators<T, T, T>
    {
        return number == T.Zero || (number & (number - T.One)) == T.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T BringNumberIntoRange<T>(T number, T min, T max) where T : INumber<T>
    {
        if (min == max)
        {
            return min;
        }

        if (sizeof(T) == sizeof(Int128))
        {
            throw new NotSupportedException();
        }

        switch (sizeof(T))
        {
            case sizeof(byte):
                short numberAsInt16 = short.CreateTruncating(number);
                short minAsInt16 = short.CreateTruncating(min);
                short maxAsInt16 = short.CreateTruncating(max);
                short rangeAsInt16 = short.Abs(short.CreateTruncating(maxAsInt16 - minAsInt16));
                return T.CreateTruncating(int.Abs(numberAsInt16 % rangeAsInt16) + minAsInt16);
            case sizeof(short):
                int numberAsInt32 = int.CreateTruncating(number);
                int minAsInt32 = int.CreateTruncating(min);
                int maxAsInt32 = int.CreateTruncating(max);
                int rangeAsInt32 = int.Abs(int.CreateTruncating(maxAsInt32 - minAsInt32));
                return T.CreateTruncating(int.Abs(numberAsInt32 % rangeAsInt32) + minAsInt32);
            case sizeof(int):
                long numberAsInt64 = long.CreateTruncating(number);
                long minAsInt64 = long.CreateTruncating(min);
                long maxAsInt64 = long.CreateTruncating(max);
                long rangeAsInt64 = long.Abs(long.CreateTruncating(maxAsInt64 - minAsInt64));
                return T.CreateTruncating(long.Abs(numberAsInt64 % rangeAsInt64) + minAsInt64);
            case sizeof(long):
                Int128 numberAsInt128 = Int128.CreateTruncating(number);
                Int128 minAsInt128 = Int128.CreateTruncating(min);
                Int128 maxAsInt128 = Int128.CreateTruncating(max);
                Int128 rangeAsInt128 = Int128.Abs(Int128.CreateTruncating(maxAsInt128 - minAsInt128));
                return T.CreateTruncating(Int128.Abs(numberAsInt128 % rangeAsInt128) + minAsInt128);
        }

        throw new UnreachableException();
    }
}
