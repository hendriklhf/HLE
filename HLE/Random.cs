using System;
using HLE.Collections;

namespace HLE;

/// <summary>
/// A static class that contains all sorts of random methods.
/// </summary>
public static class Random
{
    private const ushort _minLatinChar = 33;
    private const ushort _maxLatinChar = 126;

    public static char Char(ushort min = _minLatinChar, ushort max = _maxLatinChar)
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

        return (byte)new System.Random().Next(min, max);
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

        return (sbyte)new System.Random().Next(min, max);
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

        return (short)new System.Random().Next(min, max);
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

        return (ushort)new System.Random().Next(min, max);
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

        return new System.Random().Next(min, max);
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

        return (uint)new System.Random().NextInt64(min, max);
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

        return new System.Random().NextInt64(min, max);
    }

    public static double Double()
    {
        return new System.Random().NextDouble();
    }

    public static float Float()
    {
        return new System.Random().NextSingle();
    }

    public static string String(int length, ushort minChar = _minLatinChar, ushort maxChar = _maxLatinChar)
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

        Span<char> result = stackalloc char[length];
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
            0 => true,
            1 => false,
            _ => throw new InvalidOperationException("wtf")
        };
    }
}
