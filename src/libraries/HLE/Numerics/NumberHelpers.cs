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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ParsePositiveNumber<T>(ReadOnlySpan<char> number) where T : INumberBase<T>
    {
        T result = T.Zero;
        T ten = T.CreateTruncating(10);
        T charZero = T.CreateTruncating('0');
        ref char current = ref MemoryMarshal.GetReference(number);
        ref char end = ref Unsafe.Add(ref current, number.Length);
        do
        {
            result = ten * result + T.CreateTruncating(current) - charZero;
            current = ref Unsafe.Add(ref current, 1);
        }
        while (!Unsafe.AreSame(ref current, ref end));

        return result;
    }

    [Pure]
    public static T ParsePositiveNumber<T>(ReadOnlySpan<byte> number) where T : INumberBase<T>
    {
        T result = T.Zero;
        T ten = T.CreateTruncating(10);
        T charZero = T.CreateTruncating('0');
        ref byte current = ref MemoryMarshal.GetReference(number);
        ref byte end = ref Unsafe.Add(ref current, number.Length);
        do
        {
            result = ten * result + T.CreateTruncating(current) - charZero;
            current = ref Unsafe.Add(ref current, 1);
        }
        while (!Unsafe.AreSame(ref current, ref end));

        return result;
    }

    /// <summary>
    /// Brings a number into a range between two numbers.
    /// </summary>
    /// <remarks>
    /// Works best when <paramref name="min"/> and <paramref name="max"/> are constant values.
    /// </remarks>
    /// <param name="number">The number that will be brought into the range.</param>
    /// <param name="min">The lower bound of the range.</param>
    /// <param name="max">The upper bound of the range.</param>
    /// <typeparam name="T">The number type.</typeparam>
    /// <returns>A number between <paramref name="min"/> and exclusive <paramref name="max"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ClampMod<T>(T number, T min, T max) where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        ThrowIfNumberTypeNotSupported<T>();

        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

        if (min == max)
        {
            return min;
        }

        if (min == T.Zero)
        {
            if (max == T.Zero)
            {
                return T.Zero;
            }

            if (number >= T.Zero)
            {
                if (PopCount(max) == 1)
                {
                    return number & (max - T.One);
                }

                return number % max;
            }
        }

        if (typeof(T) == typeof(sbyte))
        {
            byte numberAsUInt8 = Unsafe.BitCast<T, byte>(number);
            byte maxAsUInt8 = Unsafe.BitCast<T, byte>(max);
            byte minAsUInt8 = Unsafe.BitCast<T, byte>(min);
            byte rangeAsUInt8 = (byte)(maxAsUInt8 - minAsUInt8);
            return T.CreateTruncating((numberAsUInt8 % rangeAsUInt8) + minAsUInt8);
        }

        if (typeof(T) == typeof(short))
        {
            ushort numberAsUInt16 = Unsafe.BitCast<T, ushort>(number);
            ushort maxAsUInt16 = Unsafe.BitCast<T, ushort>(max);
            ushort minAsUInt16 = Unsafe.BitCast<T, ushort>(min);
            ushort rangeAsUInt16 = (ushort)(maxAsUInt16 - minAsUInt16);
            return T.CreateTruncating((numberAsUInt16 % rangeAsUInt16) + minAsUInt16);
        }

        if (typeof(T) == typeof(int))
        {
            uint numberAsUInt32 = Unsafe.BitCast<T, uint>(number);
            uint maxAsUInt32 = Unsafe.BitCast<T, uint>(max);
            uint minAsUInt32 = Unsafe.BitCast<T, uint>(min);
            uint rangeAsUInt32 = maxAsUInt32 - minAsUInt32;
            return T.CreateTruncating((numberAsUInt32 % rangeAsUInt32) + minAsUInt32);
        }

        if (typeof(T) == typeof(long))
        {
            ulong numberAsUInt64 = Unsafe.BitCast<T, ulong>(number);
            ulong maxAsUInt64 = Unsafe.BitCast<T, ulong>(max);
            ulong minAsUInt64 = Unsafe.BitCast<T, ulong>(min);
            ulong rangeAsUInt64 = maxAsUInt64 - minAsUInt64;
            return T.CreateTruncating((numberAsUInt64 % rangeAsUInt64) + minAsUInt64);
        }

        if (typeof(T) == typeof(Int128))
        {
            UInt128 numberAsUInt128 = Unsafe.BitCast<T, UInt128>(number);
            UInt128 maxAsUInt128 = Unsafe.BitCast<T, UInt128>(max);
            UInt128 minAsUInt128 = Unsafe.BitCast<T, UInt128>(min);
            UInt128 rangeAsUInt128 = maxAsUInt128 - minAsUInt128;
            return T.CreateTruncating((numberAsUInt128 % rangeAsUInt128) + minAsUInt128);
        }

        T range = max - min;
        return T.CreateTruncating((number % range) + min);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int PopCount<T>(T value)
    {
        if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
        {
            return BitOperations.PopCount(Unsafe.BitCast<T, byte>(value));
        }

        if (typeof(T) == typeof(ushort) || typeof(T) == typeof(short))
        {
            return BitOperations.PopCount(Unsafe.BitCast<T, ushort>(value));
        }

        if (typeof(T) == typeof(uint) || typeof(T) == typeof(int))
        {
            return BitOperations.PopCount(Unsafe.BitCast<T, uint>(value));
        }

        if (typeof(T) == typeof(ulong) || typeof(T) == typeof(long))
        {
            return BitOperations.PopCount(Unsafe.BitCast<T, ulong>(value));
        }

        if (typeof(T) == typeof(UInt128) || typeof(T) == typeof(Int128))
        {
            ulong* v = (ulong*)&value;
            return BitOperations.PopCount(v[0]) + BitOperations.PopCount(v[1]);
        }

        ThrowHelper.ThrowUnreachableException();
        return 0;
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
            ThrowHelper.ThrowTypeNotSupported<T>();
        }
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Align<T>(T value, T alignment, AlignmentMethod method) where T : IBinaryNumber<T>
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
