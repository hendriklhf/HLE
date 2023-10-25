using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
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
    public static decimal NextDecimal(this Random random)
    {
        random.NextStruct(out decimal result);
        return result;
    }

    [Pure]
    public static string NextString(this Random random, int length)
    {
        if (length <= 0)
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
        if (length <= 0)
        {
            return string.Empty;
        }

        if (max == char.MaxValue)
        {
            return random.NextString(length);
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        if (max == 0)
        {
            return result;
        }

        random.Fill(chars);
        ref char charsReference = ref MemoryMarshal.GetReference(chars);
        BitwiseAnd(ref Unsafe.As<char, ushort>(ref charsReference), chars.Length, --max); // exclusive max
        return result;
    }

    private static void BitwiseAnd<T>(ref T values, int length, T and) where T : IBitwiseOperators<T, T, T>
    {
        int vector512Count = Vector512<T>.Count;
        if (Vector512.IsHardwareAccelerated && length >= vector512Count)
        {
            Vector512<T> andVector = Vector512.Create(and);
            while (length >= vector512Count)
            {
                Vector512<T> vector = Vector512.LoadUnsafe(ref values);
                vector &= andVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, vector512Count);
                length -= vector512Count;
            }

            if (length <= Vector512<T>.Count >> 2)
            {
                goto UnoptimizedLoop;
            }

            int remainingStart = vector512Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector512<T> remainder = Vector512.LoadUnsafe(ref values);
            remainder &= andVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        int vector256Count = Vector256<T>.Count;
        if (Vector256.IsHardwareAccelerated && length >= vector256Count)
        {
            Vector256<T> andVector = Vector256.Create(and);
            while (length >= vector256Count)
            {
                Vector256<T> vector = Vector256.LoadUnsafe(ref values);
                vector &= andVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, vector256Count);
                length -= vector256Count;
            }

            if (length <= Vector256<T>.Count >> 2)
            {
                goto UnoptimizedLoop;
            }

            int remainingStart = vector256Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector256<T> remainder = Vector256.LoadUnsafe(ref values);
            remainder &= andVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        int vector128Count = Vector128<T>.Count;
        if (Vector128.IsHardwareAccelerated && length >= vector128Count)
        {
            Vector128<T> andVector = Vector128.Create(and);
            while (length >= vector128Count)
            {
                Vector128<T> vector = Vector128.LoadUnsafe(ref values);
                vector &= andVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, vector128Count);
                length -= vector128Count;
            }

            if (length <= Vector128<T>.Count >> 2)
            {
                goto UnoptimizedLoop;
            }

            int remainingStart = vector128Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector128<T> remainder = Vector128.LoadUnsafe(ref values);
            remainder &= andVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        int vector64Count = Vector64<T>.Count;
        if (Vector64.IsHardwareAccelerated && length >= vector64Count)
        {
            Vector64<T> andVector = Vector64.Create(and);
            while (length >= vector64Count)
            {
                Vector64<T> vector = Vector64.LoadUnsafe(ref values);
                vector &= andVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, vector64Count);
                length -= vector64Count;
            }

            if (length <= Vector64<T>.Count >> 2)
            {
                goto UnoptimizedLoop;
            }

            int remainingStart = vector64Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector64<T> remainder = Vector64.LoadUnsafe(ref values);
            remainder &= andVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        UnoptimizedLoop:
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref values, i) &= and;
        }
    }

    [Pure]
    public static string NextString(this Random random, int length, char min, char max)
    {
        if (length <= 0)
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
            c = NumberHelper.BringNumberIntoRange(c, min, max);
        }

        return result;
    }

    [Pure]
    [SkipLocalsInit]
    public static string NextString(this Random random, int length, ReadOnlySpan<char> choices)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> resultSpan);
        if (choices.Length == 0)
        {
            return result;
        }

        if (!MemoryHelper.UseStackAlloc<uint>(length))
        {
            using RentedArray<uint> randomIndicesBuffer = ArrayPool<uint>.Shared.RentAsRentedArray(length);
            random.Fill(randomIndicesBuffer.AsSpan(..length));
            for (int i = 0; i < length; i++)
            {
                int randomIndex = (int)(randomIndicesBuffer[i] % choices.Length);
                resultSpan[i] = choices[randomIndex];
            }

            return result;
        }

        Span<uint> randomIndices = stackalloc uint[length];
        random.Fill(randomIndices);
        for (int i = 0; i < length; i++)
        {
            int randomIndex = (int)(randomIndices[i] % choices.Length);
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
    public static T NextStruct<T>(this Random random) where T : struct
    {
        Unsafe.SkipInit(out T result);
        Span<byte> bytes = StructMarshal<T>.GetBytes(ref result);
        random.NextBytes(bytes);
        return result;
    }

    [Pure]
    public static void NextStruct<T>(this Random random, out T result) where T : struct
    {
        Unsafe.SkipInit(out result);
        Span<byte> bytes = StructMarshal<T>.GetBytes(ref result);
        random.NextBytes(bytes);
    }

    public static unsafe void Write<T>(this Random random, T* destination, int elementCount) where T : struct
        => random.Write(ref Unsafe.AsRef<T>(destination), elementCount);

    public static unsafe void Write<T>(this Random random, ref T destination, int elementCount) where T : struct
    {
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T));
        random.NextBytes(span);
    }

    public static void Fill<T>(this Random random, T[] array) where T : struct
        => random.Fill(array.AsSpan());

    public static void Fill<T>(this Random random, Span<T> span) where T : struct
        => random.Write(ref MemoryMarshal.GetReference(span), span.Length);

    [Pure]
    public static T[] Shuffle<T>(this Random random, IEnumerable<T> collection)
    {
        T[] result;
        if (CollectionHelper.TryGetNonEnumeratedCount(collection, out int count))
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
        return NumberHelper.BringNumberIntoRange(result, min, max);
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length)
    {
        if (length <= 0)
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
        if (length <= 0)
        {
            return string.Empty;
        }

        if (max == char.MaxValue)
        {
            return random.GetString(length);
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        if (max == 0)
        {
            return result;
        }

        random.Fill(chars);
        ref char charsReference = ref MemoryMarshal.GetReference(chars);
        BitwiseAnd(ref Unsafe.As<char, ushort>(ref charsReference), chars.Length, --max); // exclusive max
        return result;
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length, char min, char max)
    {
        if (length <= 0)
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
            c = NumberHelper.BringNumberIntoRange(c, min, max);
        }

        return result;
    }

    [Pure]
    public static string GetString(this RandomNumberGenerator random, int length, ReadOnlySpan<char> choices)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        string result = StringMarshal.FastAllocateString(length, out Span<char> resultSpan);
        if (choices.Length == 0)
        {
            return result;
        }

        if (!MemoryHelper.UseStackAlloc<uint>(length))
        {
            using RentedArray<uint> randomIndicesBuffer = ArrayPool<uint>.Shared.RentAsRentedArray(length);
            random.Fill(randomIndicesBuffer.AsSpan(..length));
            for (int i = 0; i < length; i++)
            {
                int randomIndex = (int)(randomIndicesBuffer[i] % choices.Length);
                resultSpan[i] = choices[randomIndex];
            }

            return result;
        }

        Span<uint> randomIndices = stackalloc uint[length];
        random.Fill(randomIndices);
        for (int i = 0; i < length; i++)
        {
            int randomIndex = (int)(randomIndices[i] % choices.Length);
            resultSpan[i] = choices[randomIndex];
        }

        return result;
    }

    [Pure]
    public static T GetStruct<T>(this RandomNumberGenerator random) where T : struct
    {
        random.GetStruct(out T result);
        return result;
    }

    [Pure]
    public static void GetStruct<T>(this RandomNumberGenerator random, out T result) where T : struct
    {
        Unsafe.SkipInit(out result);
        Span<byte> bytes = StructMarshal<T>.GetBytes(ref result);
        random.GetBytes(bytes);
    }

    public static unsafe void Write<T>(this RandomNumberGenerator random, T* destination, int elementCount) where T : struct
        => random.Write(ref Unsafe.AsRef<T>(destination), elementCount);

    public static unsafe void Write<T>(this RandomNumberGenerator random, ref T destination, int elementCount) where T : struct
        => random.GetBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T)));

    public static void Fill<T>(this RandomNumberGenerator random, Span<T> span) where T : struct
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
