using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

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
        return digits.ToArray(..length);
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

    /// <summary>
    /// Brings a number into a range between two numbers.
    /// </summary>
    /// <param name="number">The number that will be brought into the range.</param>
    /// <param name="min">The lower bound of the range.</param>
    /// <param name="max">The upper bound of the range.</param>
    /// <typeparam name="T">The number type.</typeparam>
    /// <returns>A number between <paramref name="min"/> and exclusive <paramref name="max"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BringIntoRange<T>(T number, T min, T max) where T : INumber<T>, IMinMaxValue<T>
    {
        ThrowIfNumberTypeNotSupported<T>();

        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

        if (min == max)
        {
            return min;
        }

        if (min == T.Zero && number >= T.Zero)
        {
            return number % max;
        }

        if (typeof(T) == typeof(sbyte))
        {
            byte numberAsUInt8 = Unsafe.As<T, byte>(ref number);
            byte maxAsInt8 = Unsafe.As<T, byte>(ref max);
            byte minAsInt8 = Unsafe.As<T, byte>(ref min);
            byte rangeAsInt8 = (byte)(maxAsInt8 - minAsInt8);
            return T.CreateTruncating((numberAsUInt8 % rangeAsInt8) + minAsInt8);
        }

        if (typeof(T) == typeof(short))
        {
            ushort numberAsUInt16 = Unsafe.As<T, ushort>(ref number);
            ushort maxAsInt16 = Unsafe.As<T, ushort>(ref max);
            ushort minAsInt16 = Unsafe.As<T, ushort>(ref min);
            ushort rangeAsInt16 = (ushort)(maxAsInt16 - minAsInt16);
            return T.CreateTruncating((numberAsUInt16 % rangeAsInt16) + minAsInt16);
        }

        if (typeof(T) == typeof(int))
        {
            uint numberAsUInt32 = Unsafe.As<T, uint>(ref number);
            uint maxAsInt32 = Unsafe.As<T, uint>(ref max);
            uint minAsInt32 = Unsafe.As<T, uint>(ref min);
            uint rangeAsInt32 = maxAsInt32 - minAsInt32;
            return T.CreateTruncating((numberAsUInt32 % rangeAsInt32) + minAsInt32);
        }

        if (typeof(T) == typeof(long))
        {
            ulong numberAsUInt32 = Unsafe.As<T, ulong>(ref number);
            ulong maxAsInt32 = Unsafe.As<T, ulong>(ref max);
            ulong minAsInt32 = Unsafe.As<T, ulong>(ref min);
            ulong rangeAsInt32 = maxAsInt32 - minAsInt32;
            return T.CreateTruncating((numberAsUInt32 % rangeAsInt32) + minAsInt32);
        }

        if (typeof(T) == typeof(Int128))
        {
            UInt128 numberAsUInt32 = Unsafe.As<T, UInt128>(ref number);
            UInt128 maxAsInt32 = Unsafe.As<T, UInt128>(ref max);
            UInt128 minAsInt32 = Unsafe.As<T, UInt128>(ref min);
            UInt128 rangeAsInt32 = maxAsInt32 - minAsInt32;
            return T.CreateTruncating((numberAsUInt32 % rangeAsInt32) + minAsInt32);
        }

        T range = max - min;
        return T.CreateTruncating((number % range) + min);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNumberTypeNotSupported<T>()
    {
        if (typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) &&
            typeof(T) != typeof(ushort) && typeof(T) != typeof(short) &&
            typeof(T) != typeof(uint) && typeof(T) != typeof(int) &&
            typeof(T) != typeof(ulong) && typeof(T) != typeof(long) &&
            typeof(T) != typeof(UInt128) && typeof(T) != typeof(Int128) &&
            typeof(T) != typeof(char))
        {
            ThrowNumberTypeNotSupported<T>();
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNumberTypeNotSupported<T>()
        => throw new NotSupportedException($"The type {typeof(T)} is not supported.");

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Align<T>(T value, T alignment, AlignmentMethod method = AlignmentMethod.Add) where T : IBinaryNumber<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alignment);

        if (alignment == T.One)
        {
            return value;
        }

        return method switch
        {
            AlignmentMethod.Add => value + alignment - (value % alignment),
            AlignmentMethod.Subtract when T.IsPow2(alignment) => value & ~(alignment - T.One),
            AlignmentMethod.Subtract => value - (value % alignment),
            _ => ThrowInvalidEnumArgumentException<T>(method)
        };
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowInvalidEnumArgumentException<T>(AlignmentMethod method) where T : INumber<T>
        => throw new InvalidEnumArgumentException(nameof(method), (int)method, typeof(AlignmentMethod));

    public static void Increment<T>(Span<T> numbers) where T : INumber<T>, IMinMaxValue<T>
        => Increment(numbers, T.MinValue, T.MaxValue);

    public static void Increment<T>(Span<T> numbers, T min, T max) where T : INumber<T>
    {
        int index = numbers.LastIndexOfAnyExcept(max);
        if (index < 0)
        {
            return;
        }

        ref T numbersRef = ref MemoryMarshal.GetReference(numbers);
        for (int i = index + 1; i < numbers.Length; i++)
        {
            Unsafe.Add(ref numbersRef, i) = min;
        }

        Unsafe.Add(ref numbersRef, index)++;
    }
}
