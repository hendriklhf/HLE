using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Numerics;

namespace HLE;

public static class RandomExtensions
{
    [Pure]
    public static char NextChar(this Random random, char min = char.MinValue, char max = char.MaxValue)
        => (char)random.NextUInt16(min, max);

    [Pure]
    public static byte NextUInt8(this Random random, byte min = byte.MinValue, byte max = byte.MaxValue)
        => (byte)random.Next(min, max);

    [Pure]
    public static sbyte NextInt8(this Random random, sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
        => (sbyte)random.Next(min, max);

    [Pure]
    public static short NextInt16(this Random random, short min = short.MinValue, short max = short.MaxValue)
        => (short)random.Next(min, max);

    [Pure]
    public static ushort NextUInt16(this Random random, ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
        => (ushort)random.Next(min, max);

    [Pure]
    public static int NextInt32(this Random random, int min = int.MinValue, int max = int.MaxValue)
        => random.Next(min, max);

    [Pure]
    public static uint NextUInt32(this Random random, uint min = uint.MinValue, uint max = uint.MaxValue)
    {
        uint result = random.NextStruct<uint>();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static ulong NextUInt64(this Random random, ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
    {
        random.NextStruct(out ulong result);
        return NumberHelpers.BringIntoRange(result, min, max);
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
    public static decimal NextDecimal(this Random random)
    {
        random.NextStruct(out decimal result);
        return result;
    }

    [Pure]
    public static string NextString(this Random random, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        random.Fill(chars);
        return result;
    }

    [Pure]
    public static string NextString(this Random random, int length, char max)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

        if (max == char.MaxValue)
        {
            return random.NextString(length);
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
    public static string NextString(this Random random, int length, char min, char max)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

        if (min == 0)
        {
            return random.NextString(length, max);
        }

        if (min == char.MinValue && max == char.MaxValue)
        {
            return random.NextString(length);
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
    [SkipLocalsInit]
    public static string NextString(this Random random, int length, ReadOnlySpan<char> choices)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> resultSpan);
        if (choices.Length == 0)
        {
            return result;
        }

        uint choicesLength = (uint)choices.Length;
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
    public static bool NextBool(this Random random)
    {
        byte randomByte = (byte)(random.NextUInt8() & 1); // only return "valid" bools
        return Unsafe.As<byte, bool>(ref randomByte);
    }

    [Pure]
    public static T NextStruct<T>(this Random random) where T : unmanaged
    {
        Unsafe.SkipInit(out T result);
        Span<byte> bytes = StructMarshal.GetBytes(ref result);
        random.NextBytes(bytes);
        return result;
    }

    [Pure]
    public static void NextStruct<T>(this Random random, out T result) where T : unmanaged
    {
        Unsafe.SkipInit(out result);
        Span<byte> bytes = StructMarshal.GetBytes(ref result);
        random.NextBytes(bytes);
    }

    [Pure]
    public static TEnum NextEnumValue<TEnum>(this Random random) where TEnum : struct, Enum
    {
        ref TEnum values = ref EnumValues<TEnum>.Reference;
        int count = EnumValues<TEnum>.Count;
        return Unsafe.Add(ref values, random.Next(0, count));
    }

#pragma warning disable // formatting errors have to be disable, rider still doesnt recognize collection patterns
    private static ReadOnlySpan<uint> LeadingZeroFlagMaskValues =>
    [
        0xFFFFFFFF >>> 0, 0xFFFFFFFF >>> 1, 0xFFFFFFFF >>> 2, 0xFFFFFFFF >>> 3, 0xFFFFFFFF >>> 4,
        0xFFFFFFFF >>> 5, 0xFFFFFFFF >>> 6, 0xFFFFFFFF >>> 7, 0xFFFFFFFF >>> 8, 0xFFFFFFFF >>> 9,
        0xFFFFFFFF >>> 10, 0xFFFFFFFF >>> 11, 0xFFFFFFFF >>> 12, 0xFFFFFFFF >>> 13, 0xFFFFFFFF >>> 14,
        0xFFFFFFFF >>> 15, 0xFFFFFFFF >>> 16, 0xFFFFFFFF >>> 17, 0xFFFFFFFF >>> 18, 0xFFFFFFFF >>> 19,
        0xFFFFFFFF >>> 20, 0xFFFFFFFF >>> 21, 0xFFFFFFFF >>> 22, 0xFFFFFFFF >>> 23, 0xFFFFFFFF >>> 24,
        0xFFFFFFFF >>> 25, 0xFFFFFFFF >>> 26, 0xFFFFFFFF >>> 27, 0xFFFFFFFF >>> 28, 0xFFFFFFFF >>> 29,
        0xFFFFFFFF >>> 30, 0xFFFFFFFF >>> 31, 0
    ];
#pragma warning restore

    [Pure]
    internal static unsafe TEnum NextEnumFlags<TEnum>(this Random random) where TEnum : struct, Enum
    {
        Debug.Assert(typeof(TEnum).GetCustomAttribute<FlagsAttribute>() is not null);

        TEnum maximumEnumValue = EnumValues<TEnum>.MaximumValue;
        switch (sizeof(TEnum))
        {
            case sizeof(uint):
                uint maximumValue = Unsafe.As<TEnum, uint>(ref maximumEnumValue);
                int leadingZeroCount = BitOperations.LeadingZeroCount(maximumValue);
                uint mask = random.NextStruct<uint>();
                uint flags = LeadingZeroFlagMaskValues[leadingZeroCount] & mask;
                return Unsafe.As<uint, TEnum>(ref flags);
        }

        throw new UnreachableException();
    }

    public static unsafe void Write<T>(this Random random, T* destination, int elementCount) where T : unmanaged
        => random.Write(ref Unsafe.AsRef<T>(destination), elementCount);

    public static unsafe void Write<T>(this Random random, ref T destination, int elementCount) where T : unmanaged
    {
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T));
        random.NextBytes(span);
    }

    public static void Fill<T>(this Random random, T[] array) where T : unmanaged
        => random.Fill(array.AsSpan());

    public static void Fill<T>(this Random random, Span<T> span) where T : unmanaged
        => random.Write(ref MemoryMarshal.GetReference(span), span.Length);

    [Pure]
    public static T[] Shuffle<T>(this Random random, IEnumerable<T> collection)
    {
        T[] result;
        if (CollectionHelpers.TryGetNonEnumeratedCount(collection, out int count))
        {
            result = GC.AllocateUninitializedArray<T>(count);
            if (!collection.TryNonEnumeratedCopyTo(result))
            {
                collection.TryEnumerateInto(result, out _);
            }
        }
        else
        {
            result = collection.ToArray();
        }

        random.Shuffle(result);
        return result;
    }

    public static void Shuffle<T>(this Random random, List<T> collection)
        => random.Shuffle(CollectionsMarshal.AsSpan(collection));

    [Pure]
    public static ref T GetItem<T>(this Random random, List<T> list)
        => ref random.GetItem(CollectionsMarshal.AsSpan(list));

    [Pure]
    public static ref T GetItem<T>(this Random random, T[] array)
        => ref random.GetItem(array.AsSpan());

    [Pure]
    public static ref T GetItem<T>(this Random random, Span<T> span)
        => ref random.GetItem((ReadOnlySpan<T>)span);

    [Pure]
    public static ref T GetItem<T>(this Random random, ReadOnlySpan<T> span)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            ThrowCantGetRandomItemFromEmptyCollection();
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        int randomIndex = random.Next(spanLength);
        return ref Unsafe.Add(ref firstItem, randomIndex);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCantGetRandomItemFromEmptyCollection()
        => throw new InvalidOperationException("Can't get a random item from an empty collection.");

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
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        random.Fill(chars);
        return result;
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length, char max)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

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
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

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
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }

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
    public static ref T GetItem<T>(this RandomNumberGenerator random, List<T> list)
        => ref random.GetItem(CollectionsMarshal.AsSpan(list));

    [Pure]
    public static ref T GetItem<T>(this RandomNumberGenerator random, T[] array)
        => ref random.GetItem(array.AsSpan());

    [Pure]
    public static ref T GetItem<T>(this RandomNumberGenerator random, Span<T> span)
        => ref random.GetItem((ReadOnlySpan<T>)span);

    [Pure]
    public static ref T GetItem<T>(this RandomNumberGenerator random, ReadOnlySpan<T> span)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            ThrowCantGetRandomItemFromEmptyCollection();
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        int randomIndex = random.GetInt32(0, spanLength);
        return ref Unsafe.Add(ref firstItem, randomIndex);
    }
}
