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
using HLE.Collections;
using HLE.IL;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Numerics;

namespace HLE;

public static class RandomExtensions
{
    private static ReadOnlySpan<ulong> LeadingZeroFlagMaskValues =>
    [
        0xFFFFFFFFFFFFFFFF,
        0xFFFFFFFFFFFFFFFF >> 1,
        0xFFFFFFFFFFFFFFFF >> 2,
        0xFFFFFFFFFFFFFFFF >> 3,
        0xFFFFFFFFFFFFFFFF >> 4,
        0xFFFFFFFFFFFFFFFF >> 5,
        0xFFFFFFFFFFFFFFFF >> 6,
        0xFFFFFFFFFFFFFFFF >> 7,
        0xFFFFFFFFFFFFFFFF >> 8,
        0xFFFFFFFFFFFFFFFF >> 9,
        0xFFFFFFFFFFFFFFFF >> 10,
        0xFFFFFFFFFFFFFFFF >> 11,
        0xFFFFFFFFFFFFFFFF >> 12,
        0xFFFFFFFFFFFFFFFF >> 13,
        0xFFFFFFFFFFFFFFFF >> 14,
        0xFFFFFFFFFFFFFFFF >> 15,
        0xFFFFFFFFFFFFFFFF >> 16,
        0xFFFFFFFFFFFFFFFF >> 17,
        0xFFFFFFFFFFFFFFFF >> 18,
        0xFFFFFFFFFFFFFFFF >> 19,
        0xFFFFFFFFFFFFFFFF >> 20,
        0xFFFFFFFFFFFFFFFF >> 21,
        0xFFFFFFFFFFFFFFFF >> 22,
        0xFFFFFFFFFFFFFFFF >> 23,
        0xFFFFFFFFFFFFFFFF >> 24,
        0xFFFFFFFFFFFFFFFF >> 25,
        0xFFFFFFFFFFFFFFFF >> 26,
        0xFFFFFFFFFFFFFFFF >> 27,
        0xFFFFFFFFFFFFFFFF >> 28,
        0xFFFFFFFFFFFFFFFF >> 29,
        0xFFFFFFFFFFFFFFFF >> 30,
        0xFFFFFFFFFFFFFFFF >> 31,
        0xFFFFFFFFFFFFFFFF >> 32,
        0xFFFFFFFFFFFFFFFF >> 33,
        0xFFFFFFFFFFFFFFFF >> 34,
        0xFFFFFFFFFFFFFFFF >> 35,
        0xFFFFFFFFFFFFFFFF >> 36,
        0xFFFFFFFFFFFFFFFF >> 37,
        0xFFFFFFFFFFFFFFFF >> 38,
        0xFFFFFFFFFFFFFFFF >> 39,
        0xFFFFFFFFFFFFFFFF >> 40,
        0xFFFFFFFFFFFFFFFF >> 41,
        0xFFFFFFFFFFFFFFFF >> 42,
        0xFFFFFFFFFFFFFFFF >> 43,
        0xFFFFFFFFFFFFFFFF >> 44,
        0xFFFFFFFFFFFFFFFF >> 45,
        0xFFFFFFFFFFFFFFFF >> 46,
        0xFFFFFFFFFFFFFFFF >> 47,
        0xFFFFFFFFFFFFFFFF >> 48,
        0xFFFFFFFFFFFFFFFF >> 49,
        0xFFFFFFFFFFFFFFFF >> 50,
        0xFFFFFFFFFFFFFFFF >> 51,
        0xFFFFFFFFFFFFFFFF >> 52,
        0xFFFFFFFFFFFFFFFF >> 53,
        0xFFFFFFFFFFFFFFFF >> 54,
        0xFFFFFFFFFFFFFFFF >> 55,
        0xFFFFFFFFFFFFFFFF >> 56,
        0xFFFFFFFFFFFFFFFF >> 57,
        0xFFFFFFFFFFFFFFFF >> 58,
        0xFFFFFFFFFFFFFFFF >> 59,
        0xFFFFFFFFFFFFFFFF >> 60,
        0xFFFFFFFFFFFFFFFF >> 61,
        0xFFFFFFFFFFFFFFFF >> 62,
        0xFFFFFFFFFFFFFFFF >> 63,
        0
    ];

    [Pure]
    public static char NextChar(this Random random) => (char)random.NextUInt16();

    [Pure]
    public static char NextChar(this Random random, char min, char max)
    {
        char result = random.NextChar();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static byte NextUInt8(this Random random) => random.NextStruct<byte>();

    [Pure]
    public static byte NextUInt8(this Random random, byte min, byte max)
    {
        byte result = random.NextUInt8();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static sbyte NextInt8(this Random random) => random.NextStruct<sbyte>();

    [Pure]
    public static sbyte NextInt8(this Random random, sbyte min, sbyte max)
    {
        sbyte result = random.NextInt8();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static short NextInt16(this Random random) => random.NextStruct<short>();

    [Pure]
    public static short NextInt16(this Random random, short min, short max)
    {
        short result = random.NextInt16();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static ushort NextUInt16(this Random random) => random.NextStruct<ushort>();

    [Pure]
    public static ushort NextUInt16(this Random random, ushort min, ushort max)
    {
        ushort result = random.NextUInt16();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static int NextInt32(this Random random) => random.NextStruct<int>();

    [Pure]
    public static int NextInt32(this Random random, int min, int max)
    {
        int result = random.NextInt32();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static uint NextUInt32(this Random random) => random.NextStruct<uint>();

    [Pure]
    public static uint NextUInt32(this Random random, uint min, uint max)
    {
        uint result = random.NextUInt32();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static ulong NextUInt64(this Random random) => random.NextStruct<ulong>();

    [Pure]
    public static ulong NextUInt64(this Random random, ulong min, ulong max)
    {
        ulong result = random.NextUInt64();
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static Int128 NextInt128(this Random random)
    {
        random.NextStruct(out Int128 result);
        return result;
    }

    [Pure]
    public static Int128 NextInt128(this Random random, Int128 min, Int128 max)
    {
        random.NextStruct(out Int128 result);
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static UInt128 NextUInt128(this Random random)
    {
        random.NextStruct(out UInt128 result);
        return result;
    }

    [Pure]
    public static UInt128 NextUInt128(this Random random, UInt128 min, UInt128 max)
    {
        random.NextStruct(out UInt128 result);
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    public static Guid NextGuid(this Random random)
    {
        random.NextStruct(out Guid guid);
        return guid;
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
    public static string NextString(this Random random, int length, char max)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

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
        if (BitOperations.IsPow2(max - 1))
        {
            SpanHelpers.And(ref Unsafe.As<char, ushort>(ref charsReference), chars.Length, (char)(max - 1));
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                ref char c = ref Unsafe.Add(ref charsReference, i);
                c = NumberHelpers.BringIntoRange(c, '\0', max);
            }
        }

        return result;
    }

    [Pure]
    public static string NextString(this Random random, int length, char min, char max)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

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
        if (length == 0)
        {
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        string result = StringMarshal.FastAllocateString(length, out Span<char> chars);
        random.Fill(chars, choices);
        return result;
    }

    public static void Fill<T>(this Random random, T[] destination, ReadOnlySpan<T> choices)
        => random.Fill(ref MemoryMarshal.GetArrayDataReference(destination), destination.Length, choices);

    public static void Fill<T>(this Random random, Span<T> destination, ReadOnlySpan<T> choices)
        => random.Fill(ref MemoryMarshal.GetReference(destination), destination.Length, choices);

    [SkipLocalsInit]
    private static void Fill<T>(this Random random, ref T destination, int destinationLength, ReadOnlySpan<T> choices)
    {
        if (destinationLength == 0)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfZero(choices.Length);

        RandomWriter randomWriter = RandomWriter.Create(choices.Length);
        randomWriter.Write(random, ref destination, destinationLength, ref MemoryMarshal.GetReference(choices), choices.Length);
    }

    [Pure]
    public static bool NextBool(this Random random)
    {
        byte result = (byte)(random.NextUInt8() & 1);
        return Unsafe.As<byte, bool>(ref result);
    }

    [Pure]
    [SkipLocalsInit]
    public static T NextStruct<T>(this Random random) where T : unmanaged
    {
        Unsafe.SkipInit(out T result);
        Span<byte> bytes = StructMarshal.GetBytes(ref result);
        random.NextBytes(bytes);
        return result;
    }

    [Pure]
    [SkipLocalsInit]
    public static void NextStruct<T>(this Random random, out T result) where T : unmanaged
    {
        Unsafe.SkipInit(out result);
        Span<byte> bytes = StructMarshal.GetBytes(ref result);
        random.NextBytes(bytes);
    }

    [Pure]
    public static TEnum NextEnumValue<TEnum>(this Random random) where TEnum : struct, Enum
        => Unsafe.Add(ref EnumValues<TEnum>.Reference, random.Next(0, EnumValues<TEnum>.Count));

    [Pure]
    internal static unsafe TEnum NextEnumFlags<TEnum>(this Random random) where TEnum : struct, Enum
    {
        Debug.Assert(typeof(TEnum).GetCustomAttribute<FlagsAttribute>() is not null, $"{typeof(Enum)} is not annotated with {typeof(FlagsAttribute)}.");

        switch (sizeof(TEnum))
        {
            case sizeof(byte):
            {
                byte maximumValue = UnsafeIL.As<TEnum, byte>(EnumValues<TEnum>.MaximumValue);
                int leadingZeroCount = BitOperations.LeadingZeroCount(maximumValue);
                byte mask = random.NextStruct<byte>();
                ulong flags = LeadingZeroFlagMaskValues[56 + leadingZeroCount] & mask;
                return UnsafeIL.As<byte, TEnum>((byte)flags);
            }
            case sizeof(ushort):
            {
                ushort maximumValue = UnsafeIL.As<TEnum, ushort>(EnumValues<TEnum>.MaximumValue);
                int leadingZeroCount = BitOperations.LeadingZeroCount(maximumValue);
                ushort mask = random.NextStruct<ushort>();
                ulong flags = LeadingZeroFlagMaskValues[48 + leadingZeroCount] & mask;
                return UnsafeIL.As<ushort, TEnum>((ushort)flags);
            }
            case sizeof(uint):
            {
                uint maximumValue = UnsafeIL.As<TEnum, uint>(EnumValues<TEnum>.MaximumValue);
                int leadingZeroCount = BitOperations.LeadingZeroCount(maximumValue);
                uint mask = random.NextStruct<uint>();
                ulong flags = LeadingZeroFlagMaskValues[32 + leadingZeroCount] & mask;
                return UnsafeIL.As<uint, TEnum>((uint)flags);
            }
            case sizeof(ulong):
            {
                ulong maximumValue = UnsafeIL.As<TEnum, ulong>(EnumValues<TEnum>.MaximumValue);
                int leadingZeroCount = BitOperations.LeadingZeroCount(maximumValue);
                ulong mask = random.NextStruct<ulong>();
                ulong flags = LeadingZeroFlagMaskValues[leadingZeroCount] & mask;
                return UnsafeIL.As<uint, TEnum>((uint)flags);
            }
            default:
                ThrowHelper.ThrowUnreachableException();
                return default;
        }
    }

    public static unsafe void Write<T>(this Random random, T* destination, int elementCount) where T : unmanaged
        => random.Write(ref Unsafe.AsRef<T>(destination), elementCount);

    public static unsafe void Write<T>(this Random random, ref T destination, int elementCount) where T : unmanaged
    {
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T));
        random.NextBytes(span);
    }

    public static unsafe void Fill(this Random random, Array array)
    {
        if (ObjectMarshal.IsReferenceOrContainsReference(array.GetType().GetElementType()!))
        {
            ThrowArrayElementTypeMustBeUnmanaged();
        }

        ushort componentSize = ObjectMarshal.GetMethodTable(array)->ComponentSize;
        ref byte reference = ref MemoryMarshal.GetArrayDataReference(array);
        random.Write(ref reference, componentSize * array.Length);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowArrayElementTypeMustBeUnmanaged()
        => throw new InvalidOperationException("The array element type must be an unmanaged type.");

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
    public static ref T GetItem<T>(this Random random, List<T> items)
        => ref random.GetItem(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static ref T GetItem<T>(this Random random, T[] items)
        => ref random.GetItem(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    [Pure]
    public static ref T GetItem<T>(this Random random, Span<T> items)
        => ref random.GetItem(ref MemoryMarshal.GetReference(items), items.Length);

    [Pure]
    public static ref T GetItem<T>(this Random random, ReadOnlySpan<T> items)
        => ref random.GetItem(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T GetItem<T>(this Random random, ref T items, int length)
    {
        if (length == 0)
        {
            ThrowCantGetRandomItemFromEmptyCollection();
        }

        int randomIndex = random.Next(length);
        return ref Unsafe.Add(ref items, randomIndex);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCantGetRandomItemFromEmptyCollection()
        => throw new InvalidOperationException("Can't get a random item from an empty collection.");
}
