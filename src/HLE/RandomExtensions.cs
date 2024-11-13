using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Numerics;

namespace HLE;

public static class RandomExtensions
{
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
    [SkipLocalsInit]
    public static Int128 NextInt128(this Random random)
    {
#pragma warning disable IDE0059
        Unsafe.SkipInit(out Int128 result);
#pragma warning restore IDE0059
        random.NextStruct(out result);
        return result;
    }

    [Pure]
    [SkipLocalsInit]
    public static Int128 NextInt128(this Random random, Int128 min, Int128 max)
    {
#pragma warning disable IDE0059
        Unsafe.SkipInit(out Int128 result);
#pragma warning restore IDE0059
        random.NextStruct(out result);
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    [SkipLocalsInit]
    public static UInt128 NextUInt128(this Random random)
    {
#pragma warning disable IDE0059
        Unsafe.SkipInit(out UInt128 result);
#pragma warning restore IDE0059
        random.NextStruct(out result);
        return result;
    }

    [Pure]
    public static UInt128 NextUInt128(this Random random, UInt128 min, UInt128 max)
    {
        random.NextStruct(out UInt128 result);
        return NumberHelpers.BringIntoRange(result, min, max);
    }

    [Pure]
    [SkipLocalsInit]
    public static Guid NextGuid(this Random random)
    {
#pragma warning disable IDE0059
        Unsafe.SkipInit(out Guid guid);
#pragma warning restore IDE0059
        random.NextStruct(out guid);
        return guid;
    }

    [Pure]
    [SkipLocalsInit]
    public static decimal NextDecimal(this Random random)
    {
#pragma warning disable IDE0059
        Unsafe.SkipInit(out decimal result);
#pragma warning restore IDE0059
        random.NextStruct(out result);
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

    public static void Fill<T>(this Random random, List<T> destination, ReadOnlySpan<T> choices)
        => random.Fill(CollectionsMarshal.AsSpan(destination), choices);

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

    public static void Fill<T>(this Random random, List<T> list) where T : unmanaged
        => random.Write(ref ListMarshal.GetReference(list), (uint)list.Count);

    [RequiresDynamicCode(NativeAotMessages.RequiresDynamicCode)]
    public static unsafe void Fill(this Random random, Array array)
    {
        Type elementType = array.GetType().GetElementType()!;
        if (ObjectMarshal.IsReferenceOrContainsReferences(elementType))
        {
            ThrowArrayElementTypeMustBeUnmanaged();
        }

        Debug.Assert(ObjectMarshal.GetMethodTable(array)->HasComponentSize);

        ushort componentSize = ObjectMarshal.GetMethodTable(array)->ComponentSize;
        ref byte reference = ref MemoryMarshal.GetArrayDataReference(array);
        random.Write(ref reference, checked(componentSize * nuint.CreateChecked(array.LongLength)));

        return;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowArrayElementTypeMustBeUnmanaged()
            => throw new InvalidOperationException("The array element type must be an unmanaged type.");
    }

    public static void Fill<T>(this Random random, T[] array) where T : unmanaged
        => random.Write(ref MemoryMarshal.GetArrayDataReference(array), (uint)array.Length);

    public static void Fill<T>(this Random random, Span<T> span) where T : unmanaged
        => random.Write(ref MemoryMarshal.GetReference(span), (uint)span.Length);

    [Pure]
    public static bool NextBool(this Random random)
    {
        byte result = (byte)(random.NextUInt8() & 1);
        return Unsafe.As<byte, bool>(ref result);
    }

    [Pure]
    [SkipLocalsInit]
    public static T NextStruct<T>(this Random random)
        where T : unmanaged, allows ref struct
    {
        Unsafe.SkipInit(out T result);
        random.Write(ref result, 1);
        return result;
    }

    [Pure]
    [SkipLocalsInit]
    public static void NextStruct<T>(this Random random, out T result)
        where T : unmanaged, allows ref struct
    {
        Unsafe.SkipInit(out result);
        random.Write(ref result, 1);
    }

    [Pure]
    public static TEnum NextEnumValue<TEnum>(this Random random) where TEnum : struct, Enum
        => Unsafe.Add(ref EnumValues<TEnum>.Reference, random.Next(0, EnumValues<TEnum>.Count));

    public static unsafe void Write<T>(this Random random, T* destination, nuint elementCount)
        where T : unmanaged, allows ref struct
        => random.Write(ref Unsafe.AsRef<T>(destination), elementCount);

    public static unsafe void Write<T>(this Random random, ref T destination, nuint elementCount)
        where T : unmanaged, allows ref struct
    {
        ref byte byteDestination = ref Unsafe.As<T, byte>(ref destination);
        nuint byteCount = checked((uint)sizeof(T) * elementCount);

        if (byteCount > int.MaxValue)
        {
            WriteLoop(random, ref byteDestination, ref byteCount);
            if (byteCount == 0)
            {
                return;
            }
        }

        Debug.Assert(byteCount <= int.MaxValue);

        Span<byte> bytes = MemoryMarshal.CreateSpan(ref byteDestination, (int)byteCount);
        random.NextBytes(bytes);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // unlikely path
    private static void WriteLoop(Random random, ref byte destination, ref nuint byteCount)
    {
        Debug.Assert(byteCount > int.MaxValue);

        do
        {
            Span<byte> bytes = MemoryMarshal.CreateSpan(ref destination, int.MaxValue);
            random.NextBytes(bytes);
            destination = ref Unsafe.Add(ref destination, int.MaxValue);
            byteCount -= int.MaxValue;
        }
        while (byteCount > int.MaxValue);
    }

    [Pure]
    public static T[] Shuffle<T>(this Random random, IEnumerable<T> enumerable)
    {
        T[] items;
        if (CollectionHelpers.TryGetNonEnumeratedCount(enumerable, out int count))
        {
            items = GC.AllocateUninitializedArray<T>(count);
            if (!enumerable.TryNonEnumeratedCopyTo(items, 0, out _))
            {
                enumerable.TryEnumerateInto(items, out _);
            }
        }
        else
        {
            items = enumerable.ToArray();
        }

        random.Shuffle(items);
        return items;
    }

    public static void Shuffle<T>(this Random random, List<T> list)
        => random.Shuffle(CollectionsMarshal.AsSpan(list));

    [Pure]
    public static ref T GetItem<T>(this Random random, List<T> items)
        => ref random.GetItem(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static ref T GetItem<T>(this Random random, T[] items)
        => ref random.GetItem(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    public static ref readonly char GetItem(this Random random, string str)
        => ref random.GetItem(ref StringMarshal.GetReference(str), str.Length);

    [Pure]
    public static ref T GetItem<T>(this Random random, Span<T> items)
        => ref random.GetItem(ref MemoryMarshal.GetReference(items), items.Length);

    [Pure]
    public static ref readonly T GetItem<T>(this Random random, ReadOnlySpan<T> items)
        => ref random.GetItem(ref MemoryMarshal.GetReference(items), items.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T GetItem<T>(this Random random, ref T items, int length)
        where T : allows ref struct
    {
        if (length == 0)
        {
            ThrowCantGetRandomItemFromEmptyCollection();
        }

        int randomIndex = random.Next(length);
        return ref Unsafe.Add(ref items, randomIndex);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowCantGetRandomItemFromEmptyCollection()
            => throw new InvalidOperationException("Can't get a random item from an empty collection.");
    }
}
