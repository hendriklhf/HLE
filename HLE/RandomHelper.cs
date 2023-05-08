using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using HLE.Collections;
using HLE.Memory;
using HLE.Numerics;

namespace HLE;

public static class RandomHelper
{
    [Pure]
    public static char NextChar(this Random random, char min = char.MinValue, char max = char.MaxValue)
    {
        return (char)random.NextUInt16(min, max);
    }

    [Pure]
    public static byte NextUInt8(this Random random, byte min = byte.MinValue, byte max = byte.MaxValue)
    {
        byte result = random.NextStruct<byte>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static sbyte NextInt8(this Random random, sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
    {
        sbyte result = random.NextStruct<sbyte>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static short NextInt16(this Random random, short min = short.MinValue, short max = short.MaxValue)
    {
        return (short)random.Next(min, max);
    }

    [Pure]
    public static ushort NextUInt16(this Random random, ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
    {
        return (ushort)random.Next(min, max);
    }

    [Pure]
    public static int NextInt32(this Random random, int min = int.MinValue, int max = int.MaxValue)
    {
        return random.Next(min, max);
    }

    [Pure]
    public static uint NextUInt32(this Random random, uint min = uint.MinValue, uint max = uint.MaxValue)
    {
        uint result = random.NextStruct<uint>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static ulong NextUInt64(this Random random, ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
    {
        random.NextStruct(out ulong result);
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static Int128 NextInt128(this Random random)
    {
        random.NextStruct(out Int128 result);
        return result;
    }

    [Pure]
    public static UInt128 NextUInt128(this Random random)
    {
        random.NextStruct(out UInt128 result);
        return result;
    }

    [Pure]
    // ReSharper disable once UnusedParameter.Global
    public static unsafe string NextString(this Random random, int length, char minChar = char.MinValue, char maxChar = char.MaxValue)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        ushort charCount = (ushort)(maxChar - minChar);
        Span<ushort> chars = MemoryHelper.UseStackAlloc<ushort>(charCount) ? stackalloc ushort[charCount] : new ushort[charCount];
        CollectionHelper.FillAscending(chars, minChar);

        if (!MemoryHelper.UseStackAlloc<ushort>(length))
        {
            using RentedArray<ushort> bufferResult = new(length);
            for (int i = 0; i < length; i++)
            {
                bufferResult[i] = chars.Random();
            }

            return new((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bufferResult.Span)), 0, length);
        }

        ushort* result = stackalloc ushort[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars.Random();
        }

        return new((char*)result, 0, length);
    }

    public static string NextString(this Random random, int length, ReadOnlySpan<char> chars)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        if (!MemoryHelper.UseStackAlloc<int>(length))
        {
            using RentedArray<char> resultBuffer = new(length);
            using RentedArray<int> randomIndicesBuffer = new(length);
            random.Fill(randomIndicesBuffer.Span);
            for (int i = 0; i < length; i++)
            {
                int randomIndex = NumberHelper.SetSignBitToZero(randomIndicesBuffer[i]) % chars.Length;
                resultBuffer[i] = chars[randomIndex];
            }

            return new(resultBuffer[..length]);
        }

        Span<char> result = stackalloc char[length];
        Span<int> randomIndices = stackalloc int[length];
        random.Fill(randomIndices);
        for (int i = 0; i < length; i++)
        {
            int randomIndex = NumberHelper.SetSignBitToZero(randomIndices[i]) % chars.Length;
            result[i] = chars[randomIndex];
        }

        return new(result);
    }

    [Pure]
    public static bool NextBool(this Random random)
    {
        byte randomByte = random.NextUInt8();
        return Unsafe.As<byte, bool>(ref randomByte);
    }

    [Pure]
    public static unsafe T NextStruct<T>(this Random random) where T : struct
    {
        Unsafe.SkipInit(out T result);
        Span<byte> bytes = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        random.NextBytes(bytes);
        return result;
    }

    [Pure]
    public static unsafe void NextStruct<T>(this Random random, out T result) where T : struct
    {
        Unsafe.SkipInit(out result);
        Span<byte> bytes = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        random.NextBytes(bytes);
    }

    public static unsafe void Write<T>(this Random random, T* destination, int elementCount) where T : struct
    {
        random.Write(ref Unsafe.AsRef<T>(destination), elementCount);
    }

    public static unsafe void Write<T>(this Random random, ref T destination, int elementCount) where T : struct
    {
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T));
        random.NextBytes(span);
    }

    public static void Fill<T>(this Random random, Span<T> span) where T : struct
    {
        random.Write(ref MemoryMarshal.GetReference(span), span.Length);
    }

    [Pure]
    public static bool GetBool(this RandomNumberGenerator random)
    {
        byte result = random.GetUInt8();
        return Unsafe.As<byte, bool>(ref result);
    }

    [Pure]
    public static byte GetUInt8(this RandomNumberGenerator random, byte min = byte.MinValue, byte max = byte.MaxValue)
    {
        byte result = random.GetStruct<byte>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static sbyte GetInt8(this RandomNumberGenerator random, sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
    {
        sbyte result = random.GetStruct<sbyte>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static short GetInt16(this RandomNumberGenerator random, short min = short.MinValue, short max = short.MaxValue)
    {
        short result = random.GetStruct<short>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static ushort GetUInt16(this RandomNumberGenerator random, ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
    {
        ushort result = random.GetStruct<ushort>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static int GetInt32(this RandomNumberGenerator random, int min = int.MinValue, int max = int.MaxValue)
    {
        int result = random.GetStruct<int>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static uint GetUInt32(this RandomNumberGenerator random, uint min = uint.MinValue, uint max = uint.MaxValue)
    {
        uint result = random.GetStruct<uint>();
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static long GetInt64(this RandomNumberGenerator random, long min = long.MinValue, long max = long.MaxValue)
    {
        random.GetStruct(out long result);
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static ulong GetUInt64(this RandomNumberGenerator random, ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
    {
        random.GetStruct(out ulong result);
        return NumberHelper.BringNumberIntoRange(result, min, max);
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
    public static float GetFloat(this RandomNumberGenerator random)
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
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static unsafe string GetString(this RandomNumberGenerator random, int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        Span<char> chars = MemoryHelper.UseStackAlloc<char>(length) ? stackalloc char[length] : new char[length];
        Span<byte> bytes = MemoryMarshal.CreateSpan(ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(chars)), length * sizeof(char));
        random.GetBytes(bytes);
        return new((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)), 0, length);
    }

    [Pure]
    public static unsafe T GetStruct<T>(this RandomNumberGenerator random) where T : struct
    {
        Unsafe.SkipInit(out T result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        random.GetBytes(span);
        return result;
    }

    [Pure]
    public static unsafe void GetStruct<T>(this RandomNumberGenerator random, out T result) where T : struct
    {
        Unsafe.SkipInit(out result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        random.GetBytes(span);
    }

    public static unsafe void Write<T>(this RandomNumberGenerator random, T* destination, int elementCount) where T : struct
    {
        random.Write(ref Unsafe.AsRef<T>(destination), elementCount);
    }

    public static unsafe void Write<T>(this RandomNumberGenerator random, ref T destination, int elementCount) where T : struct
    {
        random.GetBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T)));
    }

    public static void Fill<T>(this RandomNumberGenerator random, Span<T> span) where T : struct
    {
        random.Write(ref MemoryMarshal.GetReference(span), span.Length);
    }
}
