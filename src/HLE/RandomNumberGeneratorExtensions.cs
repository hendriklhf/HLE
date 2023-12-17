using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Numerics;

namespace HLE;

public static class RandomNumberGeneratorExtensions
{
    [Pure]
    public static bool GetBool(this RandomNumberGenerator random)
    {
        byte result = (byte)(random.GetUInt8() & 1); // only return "valid" bools
        return Unsafe.As<byte, bool>(ref result);
    }

    [Pure]
    public static byte GetUInt8(this RandomNumberGenerator random, byte min = byte.MinValue, byte max = byte.MaxValue)
    {
        byte result = random.GetStruct<byte>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static sbyte GetInt8(this RandomNumberGenerator random, sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
    {
        sbyte result = random.GetStruct<sbyte>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static short GetInt16(this RandomNumberGenerator random, short min = short.MinValue, short max = short.MaxValue)
    {
        short result = random.GetStruct<short>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static ushort GetUInt16(this RandomNumberGenerator random, ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
    {
        ushort result = random.GetStruct<ushort>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static int GetInt32(this RandomNumberGenerator random, int min = int.MinValue, int max = int.MaxValue)
    {
        int result = random.GetStruct<int>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static uint GetUInt32(this RandomNumberGenerator random, uint min = uint.MinValue, uint max = uint.MaxValue)
    {
        uint result = random.GetStruct<uint>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static long GetInt64(this RandomNumberGenerator random, long min = long.MinValue, long max = long.MaxValue)
    {
        random.GetStruct(out long result);
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static ulong GetUInt64(this RandomNumberGenerator random, ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
    {
        random.GetStruct(out ulong result);
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static Int128 GetInt128(this RandomNumberGenerator random)
    {
        random.GetStruct(out Int128 result);
        return result;
    }

    [Pure]
    public static UInt128 GetUInt128(this RandomNumberGenerator random)
    {
        random.GetStruct(out UInt128 result);
        return result;
    }

    [Pure]
    public static float GetSingle(this RandomNumberGenerator random)
    {
        random.GetStruct(out float result);
        return result;
    }

    [Pure]
    public static double GetDouble(this RandomNumberGenerator random)
    {
        random.GetStruct(out double result);
        return result;
    }

    [Pure]
    public static decimal GetDecimal(this RandomNumberGenerator random)
    {
        random.GetStruct(out decimal result);
        return result;
    }

    [Pure]
    public static char GetChar(this RandomNumberGenerator random, char min = char.MinValue, char max = char.MaxValue)
    {
        char result = random.GetStruct<char>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        random.Fill(chars);
        return result;
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length, char max)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (max == char.MaxValue)
        {
            return random.GetString(length);
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        if (max <= 1)
        {
            return result;
        }

        random.Fill(chars);
        ref char charsReference = ref MemoryMarshal.GetReference(chars);
        SpanHelpers.BitwiseAnd(ref Unsafe.As<char, ushort>(ref charsReference), chars.Length, --max); // exclusive max
        return result;
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length, char min, char max)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (min == 0)
        {
            return random.GetString(length, max);
        }

        if (min == char.MinValue && max == char.MaxValue)
        {
            return random.GetString(length);
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        if (min == char.MaxValue)
        {
            chars.Fill(char.MaxValue);
            return result;
        }

        if (min == max)
        {
            chars.Fill(min);
            return result;
        }

        ref char charsReference = ref MemoryMarshal.GetReference(chars);

        random.Fill(chars);
        for (int i = 0; i < length; i++)
        {
            ref char c = ref Unsafe.Add(ref charsReference, i);
            c = NumberHelpers.BringIntoRange(c, min, max);
        }

        return result;
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length, ReadOnlySpan<char> choices)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        string result = StringMarshal.FastAllocateString(length, out Span<char> resultSpan);
        uint choicesLength = (uint)choices.Length;
        if (choicesLength == 0)
        {
            return result;
        }

        if (!MemoryHelpers.UseStackAlloc<uint>(length))
        {
            using RentedArray<uint> randomIndicesBuffer = ArrayPool<uint>.Shared.RentAsRentedArray(length);
            random.Fill(randomIndicesBuffer.AsSpan(..length));
            for (int i = 0; i < length; i++)
            {
                int randomIndex = (int)(randomIndicesBuffer[i] % choicesLength);
                resultSpan[i] = choices[randomIndex];
            }

            return result;
        }

        Span<uint> randomIndices = stackalloc uint[length];
        random.Fill(randomIndices);
        for (int i = 0; i < length; i++)
        {
            int randomIndex = (int)(randomIndices[i] % choicesLength);
            resultSpan[i] = choices[randomIndex];
        }

        return result;
    }

    [Pure]
    public static T GetStruct<T>(this RandomNumberGenerator random) where T : unmanaged
    {
        random.GetStruct(out T result);
        return result;
    }

    [Pure]
    public static void GetStruct<T>(this RandomNumberGenerator random, out T result) where T : unmanaged
    {
        Unsafe.SkipInit(out result);
        Span<byte> bytes = StructMarshal.GetBytes(ref result);
        random.GetBytes(bytes);
    }

    public static unsafe void Write<T>(this RandomNumberGenerator random, T* destination, int elementCount) where T : unmanaged
        => random.Write(ref Unsafe.AsRef<T>(destination), elementCount);

    public static unsafe void Write<T>(this RandomNumberGenerator random, ref T destination, int elementCount) where T : unmanaged
        => random.GetBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T)));

    public static void Fill<T>(this RandomNumberGenerator random, Span<T> span) where T : unmanaged
        => random.Write(ref MemoryMarshal.GetReference(span), span.Length);

    [Pure]
    public static ref T GetItem<T>(this RandomNumberGenerator random, List<T> items)
        => ref random.GetItem(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static ref T GetItem<T>(this RandomNumberGenerator random, T[] items)
        => ref random.GetItem(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    [Pure]
    public static ref T GetItem<T>(this RandomNumberGenerator random, Span<T> items)
        => ref random.GetItem(ref MemoryMarshal.GetReference(items), items.Length);

    [Pure]
    public static ref T GetItem<T>(this RandomNumberGenerator random, ReadOnlySpan<T> items)
        => ref random.GetItem(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T GetItem<T>(this RandomNumberGenerator random, ref T items, int length)
    {
        if (length == 0)
        {
            ThrowCantGetRandomItemFromEmptyCollection();
        }

        int randomIndex = random.GetInt32(0, length);
        return ref Unsafe.Add(ref items, randomIndex);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCantGetRandomItemFromEmptyCollection()
        => throw new InvalidOperationException("Can't get a random item from an empty collection.");
}
