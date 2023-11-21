using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Numerics;

public static class NumberHelpers
{
    [Pure]
    public static int GetNumberLength<T>(T number) where T : INumber<T>
        => number == T.Zero ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(double.CreateTruncating(number))) + 1);

    [Pure]
    [SkipLocalsInit]
    public static byte[] GetDigits<T>(T number) where T : INumber<T>
    {
        Span<byte> digits = stackalloc byte[50];
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

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char DigitToChar(byte digit) => (char)(digit + (byte)'0');

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte CharToDigit(char c) => (byte)(c - '0');

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte CharToDigit(byte c) => (byte)(c - '0');

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
        T charZero = T.CreateTruncating('0');
        for (int i = 0; i < number.Length; i++)
        {
            result = ten * result + T.CreateTruncating(number[i]) - charZero;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T BringNumberIntoRange<T>(T number, T min, T max) where T : INumber<T>
    {
        if (sizeof(T) == sizeof(Int128))
        {
            Throw128BitIntegerNotSupported();
        }

        if (min == max)
        {
            return min;
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

        return ThrowUnreachableException<T>("This shouldn't happen, as all number types are covered.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowUnreachableException<T>(string message) => throw new UnreachableException(message);

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Throw128BitIntegerNotSupported()
        => throw new NotSupportedException($"{typeof(Int128)} and {typeof(UInt128)} are not supported.");

    [Pure]
    public static T Align<T>(T number, T alignment, AlignmentMethod method = AlignmentMethod.Add) where T : INumber<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alignment);

        return method switch
        {
            AlignmentMethod.Add => number + alignment - (number % alignment),
            AlignmentMethod.Subtract => number - (number % alignment),
            _ => ThrowInvalidEnumArgumentException<T>(method)
        };
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowInvalidEnumArgumentException<T>(AlignmentMethod method) where T : INumber<T>
        => throw new InvalidEnumArgumentException(nameof(method), (int)method, typeof(AlignmentMethod));
}
