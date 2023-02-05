using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using HLE.Collections;

namespace HLE;

/// <summary>
/// A static class that contains all sorts of random methods.
/// </summary>
public static class Random
{
    private static readonly System.Random _rng = new();
    private static readonly RandomNumberGenerator _strong = RandomNumberGenerator.Create();

    private const ushort _minAsciiPrintableChar = 32;
    private const ushort _maxAsciiPrintableChar = 126;

    public static char Char(ushort min = _minAsciiPrintableChar, ushort max = _maxAsciiPrintableChar)
    {
        return (char)UShort(min, max);
    }

    public static byte Byte(byte min = byte.MinValue, byte max = byte.MaxValue)
    {
        if (min > max)
        {
            (max, min) = (min, max);
        }

        if (max < byte.MaxValue)
        {
            max++;
        }

        return (byte)_rng.Next(min, max);
    }

    public static sbyte SByte(sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
    {
        if (min > max)
        {
            (max, min) = (min, max);
        }

        if (max < sbyte.MaxValue)
        {
            max++;
        }

        return (sbyte)_rng.Next(min, max);
    }

    public static short Short(short min = short.MinValue, short max = short.MaxValue)
    {
        if (min > max)
        {
            (max, min) = (min, max);
        }

        if (max < short.MaxValue)
        {
            max++;
        }

        return (short)_rng.Next(min, max);
    }

    public static ushort UShort(ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
    {
        if (min > max)
        {
            (max, min) = (min, max);
        }

        if (max < ushort.MaxValue)
        {
            max++;
        }

        return (ushort)_rng.Next(min, max);
    }

    /// <summary>
    /// Returns a random <see cref="int"/> between the given borders.<br />
    /// Default values are <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>.<br />
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value</param>
    /// <returns>A random <see cref="int"/>.</returns>
    public static int Int(int min = int.MinValue, int max = int.MaxValue)
    {
        if (min > max)
        {
            (max, min) = (min, max);
        }

        if (max < int.MaxValue)
        {
            max++;
        }

        return _rng.Next(min, max);
    }

    public static uint UInt(uint min = uint.MinValue, uint max = uint.MaxValue)
    {
        if (min > max)
        {
            (max, min) = (min, max);
        }

        if (max < uint.MaxValue)
        {
            max++;
        }

        return (uint)_rng.NextInt64(min, max);
    }

    public static long Long(long min = long.MinValue, long max = long.MaxValue)
    {
        if (min > max)
        {
            (max, min) = (min, max);
        }

        if (max < long.MaxValue)
        {
            max++;
        }

        return _rng.NextInt64(min, max);
    }

    public static double Double()
    {
        return _rng.NextDouble();
    }

    public static float Float()
    {
        return _rng.NextSingle();
    }

    public static string String(int length, ushort minChar = _minAsciiPrintableChar, ushort maxChar = _maxAsciiPrintableChar)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        ushort count = (ushort)(maxChar - minChar);
        Span<char> chars = stackalloc char[count];
        for (int i = 0; i < count; i++)
        {
            chars[i] = (char)(i + minChar);
        }

        Span<char> result = MemoryHelper.UseStackAlloc<char>(length) ? stackalloc char[length] : new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars.Random();
        }

        return new(result);
    }

    public static bool Bool()
    {
        return Byte(0, 1) switch
        {
            0 => false,
            1 => true,
            _ => throw new UnreachableException()
        };
    }

    public static unsafe T Struct<T>() where T : struct
    {
        int sizeT = sizeof(T);
        Span<byte> bytes = MemoryHelper.UseStackAlloc<T>(1) ? stackalloc byte[sizeT] : new byte[sizeT];
        ref byte firstByte = ref MemoryMarshal.GetReference(bytes);
        for (int i = 0; i < sizeT; i++)
        {
            Unsafe.Add(ref firstByte, i) = Byte();
        }

        return Unsafe.As<byte, T>(ref firstByte);
    }

    public static unsafe void Write<T>(T* destination, int elementCount)
    {
        Span<byte> bytes = new(destination, elementCount * sizeof(T));
        int byteCount = bytes.Length;
        for (int i = 0; i < byteCount; i++)
        {
            bytes[i] = Byte();
        }
    }

    public static unsafe void Write<T>(ref T destination, int elementCount)
    {
        T* pointer = (T*)Unsafe.AsPointer(ref destination);
        Write(pointer, elementCount);
    }

    public static bool StrongBool()
    {
        return StrongSByte() switch
        {
            >= 0 => true,
            < 0 => false
        };
    }

    public static byte StrongByte()
    {
        Span<byte> bytes = stackalloc byte[sizeof(byte)];
        _strong.GetBytes(bytes);
        return bytes[0];
    }

    public static sbyte StrongSByte()
    {
        Span<byte> bytes = stackalloc byte[sizeof(byte)];
        _strong.GetBytes(bytes);
        return (sbyte)bytes[0];
    }

    public static short StrongShort()
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, short>(ref MemoryMarshal.GetReference(bytes));
    }

    public static ushort StrongUShort()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, ushort>(ref MemoryMarshal.GetReference(bytes));
    }

    public static int StrongInt()
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, int>(ref MemoryMarshal.GetReference(bytes));
    }

    public static uint StrongUInt()
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, uint>(ref MemoryMarshal.GetReference(bytes));
    }

    public static long StrongLong()
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, long>(ref MemoryMarshal.GetReference(bytes));
    }

    public static ulong StrongULong()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(bytes));
    }

    public static unsafe Int128 StrongInt128()
    {
        Span<byte> bytes = stackalloc byte[sizeof(Int128)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, Int128>(ref MemoryMarshal.GetReference(bytes));
    }

    public static unsafe UInt128 StrongUInt128()
    {
        Span<byte> bytes = stackalloc byte[sizeof(Int128)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, UInt128>(ref MemoryMarshal.GetReference(bytes));
    }

    public static float StrongFloat()
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, float>(ref MemoryMarshal.GetReference(bytes));
    }

    public static double StrongDouble()
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, double>(ref MemoryMarshal.GetReference(bytes));
    }

    public static decimal StrongDecimal()
    {
        Span<byte> bytes = stackalloc byte[sizeof(decimal)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, decimal>(ref MemoryMarshal.GetReference(bytes));
    }

    public static char StrongChar()
    {
        return (char)StrongUShort();
    }

    public static unsafe string StrongString(int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        Span<byte> bytes = MemoryHelper.UseStackAlloc<char>(length) ? stackalloc byte[length * sizeof(char)] : new byte[length * sizeof(char)];
        _strong.GetBytes(bytes);
        return new((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)), 0, length);
    }

    public static unsafe T StrongStruct<T>() where T : struct
    {
        Span<byte> bytes = MemoryHelper.UseStackAlloc<T>(1) ? stackalloc byte[sizeof(T)] : new byte[sizeof(T)];
        _strong.GetBytes(bytes);
        return Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(bytes));
    }

    public static unsafe void WriteStrong<T>(T* destination, int elementCount)
    {
        Span<byte> bytes = new(destination, elementCount * sizeof(T));
        _strong.GetBytes(bytes);
    }

    public static unsafe void WriteStrong<T>(ref T destination, int elementCount)
    {
        T* pointer = (T*)Unsafe.AsPointer(ref destination);
        WriteStrong(pointer, elementCount);
    }
}
