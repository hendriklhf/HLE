using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using HLE.Collections;
using HLE.Memory;

namespace HLE;

/// <summary>
/// A static class that contains all sorts of random methods.
/// </summary>
public static class Random
{
    private static readonly System.Random _weak = new();
    private static readonly RandomNumberGenerator _strong = RandomNumberGenerator.Create();

    private const char _minAsciiPrintableChar = (char)32;
    private const char _maxAsciiPrintableChar = (char)126;

    [Pure]
    public static char Char(char min = _minAsciiPrintableChar, char max = _maxAsciiPrintableChar)
    {
        return (char)UShort(min, max);
    }

    [Pure]
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

        return (byte)_weak.Next(min, max);
    }

    [Pure]
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

        return (sbyte)_weak.Next(min, max);
    }

    [Pure]
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

        return (short)_weak.Next(min, max);
    }

    [Pure]
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

        return (ushort)_weak.Next(min, max);
    }

    /// <summary>
    /// Returns a random <see cref="int"/> between the given borders.<br />
    /// Default values are <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>.<br />
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value</param>
    /// <returns>A random <see cref="int"/>.</returns>
    [Pure]
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

        return _weak.Next(min, max);
    }

    [Pure]
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

        return (uint)_weak.NextInt64(min, max);
    }

    [Pure]
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

        return _weak.NextInt64(min, max);
    }

    [Pure]
    public static double Double()
    {
        return _weak.NextDouble();
    }

    [Pure]
    public static float Float()
    {
        return _weak.NextSingle();
    }

    [Pure]
    public static string String(int length, char minChar = _minAsciiPrintableChar, char maxChar = _maxAsciiPrintableChar)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        ushort charCount = (ushort)(maxChar - minChar);
        Span<char> chars = MemoryHelper.UseStackAlloc<char>(charCount) ? stackalloc char[charCount] : new char[charCount];
        for (int i = 0; i < charCount; i++)
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

    [Pure]
    public static bool Bool()
    {
        byte randomByte = Byte(0, 1);
        return Unsafe.As<byte, bool>(ref randomByte);
    }

    [Pure]
    public static unsafe T Struct<T>() where T : struct
    {
        Unsafe.SkipInit(out T result);
        Span<byte> bytes = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        _weak.NextBytes(bytes);
        return result;
    }

    [Pure]
    public static unsafe void Struct<T>(out T result) where T : struct
    {
        Unsafe.SkipInit(out result);
        Span<byte> bytes = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        _weak.NextBytes(bytes);
    }

    public static unsafe void Write<T>(T* destination, int elementCount) where T : struct
    {
        Write(ref Unsafe.AsRef<T>(destination), elementCount);
    }

    public static unsafe void Write<T>(ref T destination, int elementCount) where T : struct
    {
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), elementCount * sizeof(T));
        _weak.NextBytes(span);
    }

    [Pure]
    public static bool StrongBool()
    {
        byte result = (byte)(StrongByte() & 1);
        return Unsafe.As<byte, bool>(ref result);
    }

    [Pure]
    public static byte StrongByte()
    {
        Unsafe.SkipInit(out byte result);
        Span<byte> span = new(ref result);
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static sbyte StrongSByte()
    {
        Unsafe.SkipInit(out sbyte result);
        Span<byte> span = new(ref Unsafe.As<sbyte, byte>(ref result));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static short StrongShort()
    {
        Unsafe.SkipInit(out short result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<short, byte>(ref result), sizeof(short));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static ushort StrongUShort()
    {
        Unsafe.SkipInit(out ushort result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<ushort, byte>(ref result), sizeof(ushort));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static int StrongInt()
    {
        Unsafe.SkipInit(out int result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<int, byte>(ref result), sizeof(int));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static uint StrongUInt()
    {
        Unsafe.SkipInit(out uint result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<uint, byte>(ref result), sizeof(uint));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static long StrongLong()
    {
        Unsafe.SkipInit(out long result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<long, byte>(ref result), sizeof(long));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static ulong StrongULong()
    {
        Unsafe.SkipInit(out ulong result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<ulong, byte>(ref result), sizeof(ulong));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static unsafe Int128 StrongInt128()
    {
        Unsafe.SkipInit(out Int128 result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<Int128, byte>(ref result), sizeof(Int128));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static unsafe UInt128 StrongUInt128()
    {
        Unsafe.SkipInit(out UInt128 result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<UInt128, byte>(ref result), sizeof(UInt128));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static float StrongFloat()
    {
        Unsafe.SkipInit(out float result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<float, byte>(ref result), sizeof(float));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static double StrongDouble()
    {
        Unsafe.SkipInit(out double result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<double, byte>(ref result), sizeof(double));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static decimal StrongDecimal()
    {
        Unsafe.SkipInit(out decimal result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<decimal, byte>(ref result), sizeof(decimal));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static char StrongChar()
    {
        return (char)StrongUShort();
    }

    [Pure]
    public static unsafe string StrongString(int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        Span<char> chars = MemoryHelper.UseStackAlloc<char>(length) ? stackalloc char[length] : new char[length];
        Span<byte> bytes = MemoryMarshal.CreateSpan(ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(chars)), length * sizeof(char));
        _strong.GetBytes(bytes);
        return new((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)), 0, length);
    }

    [Pure]
    public static unsafe T StrongStruct<T>() where T : struct
    {
        Unsafe.SkipInit(out T result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        _strong.GetBytes(span);
        return result;
    }

    [Pure]
    public static unsafe void StrongStruct<T>(out T result) where T : struct
    {
        Unsafe.SkipInit(out result);
        Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T));
        _strong.GetBytes(span);
    }

    public static unsafe void WriteStrong<T>(T* destination, int elementCount) where T : struct
    {
        WriteStrong(ref Unsafe.AsRef<T>(destination), elementCount);
    }

    public static void WriteStrong<T>(ref T destination, int elementCount) where T : struct
    {
        for (int i = 0; i < elementCount; i++)
        {
            Unsafe.Add(ref destination, i) = StrongStruct<T>();
        }
    }
}
