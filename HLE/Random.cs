using System;
using System.Linq;
using System.Text;
using HLE.Collections;

namespace HLE;

/// <summary>
/// A static class that contains all sorts of random methods.
/// </summary>
public static class Random
{
    /// <summary>
    /// Returns a random <see cref="char"/> out of all basic latin characters.
    /// </summary>
    /// <returns>A basic Latin character.</returns>
    public static char Char()
    {
        return CharCollection.BasicLatinChars.Random();
    }

    public static char Char(ushort min, ushort max)
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

    /// <summary>
    /// Returns a <see cref="string"/> of the given <paramref name="length"/> filled with basic Latin characters.<br />
    /// Calls <see cref="Char()"/> to fill the result string.
    /// </summary>
    /// <param name="length">The <paramref name="length"/> of the <see cref="string"/>.</param>
    /// <returns>A string of the given <paramref name="length"/>.</returns>
    public static string String(int length = 10)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        for (int i = 0; i < length; i++)
        {
            builder.Append(Char());
        }

        return builder.ToString();
    }

    public static string String(int length, ushort minChar, ushort maxChar)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        ushort[] chars = NumberCollection.Create(minChar, maxChar).ToArray();
        StringBuilder builder = new();
        for (int i = 0; i < length; i++)
        {
            builder.Append((char)chars.Random());
        }

        return builder.ToString();
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
