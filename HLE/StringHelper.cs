using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HLE;

/// <summary>
/// A class to help with any kind of <see cref="string"/>.
/// </summary>
public static class StringHelper
{
    private static readonly Regex _multipleSpacesPattern = new(@"\s+", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// This char moves the chars to the right of this char to the left in Twitch chat.
    /// </summary>
    public const char ZeroWidthChar = '�';

    /// <summary>
    /// This char is not visible in Twitch chat.
    /// </summary>
    public const char InvisibleChar = '⠀';

    public static string Decode(this byte[] bytes, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return encoding.GetString(bytes);
    }

    public static byte[] Encode(this string str, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return encoding.GetBytes(str);
    }

    /// <summary>
    /// Removes the given <see cref="string"/> <paramref name="s"/> from the input <see cref="string"/> <paramref name="str"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> from with the given <see cref="string"/> <paramref name="s"/> will be removed.</param>
    /// <param name="s">The <see cref="string"/> that will be removed from the input <see cref="string"/> <paramref name="str"/>.</param>
    /// <returns>Returns the <see cref="string"/> <paramref name="str"/> with the <paramref name="s"/> removed.</returns>
    public static string Remove(this string str, string s)
    {
        return str.Replace(s, "");
    }

    public static IEnumerable<string> Split(this string str, int charCount, bool onlySplitOnWhitespace = false)
    {
        str = str.TrimAll();
        if (str.Length <= charCount)
        {
            return new[]
            {
                str
            };
        }

        if (!onlySplitOnWhitespace)
        {
            List<string> result = new();
            while (str.Length > charCount)
            {
                result.Add(str[..charCount]);
                str = str[charCount..];
            }

            result.Add(str);
            return result;
        }
        else
        {
            string[] split = str.Split();
            List<List<string>?> list = new();
            int listIdx = 0;
            for (int i = 0; i < split.Length; i++)
            {
                if (list.Count < listIdx + 1)
                {
                    list.Add(new());
                }

                list[listIdx] ??= new();

                bool doesntExceedMaxCharCount = list[listIdx]!.Select(st => st.Length).Sum() + list[listIdx]!.Count + split[i].Length <= charCount;
                if (doesntExceedMaxCharCount)
                {
                    list[listIdx]!.Add(split[i]);
                }
                else
                {
                    listIdx++;
                    i--;
                }
            }

            List<string> result = new();
            foreach (List<string>? l in list)
            {
                if (l is not null)
                {
                    result.Add(string.Join(' ', l));
                }
            }

            return result;
        }
    }

    public static string TakeBetween(this string str, char firstChar, char secondChar)
    {
        int firstIdx = str.IndexOf(firstChar);
        int secondIdx = str.IndexOf(secondChar);
        Range range;
        switch (firstIdx)
        {
            case -1 when secondIdx == -1:
                range = Range.All;
                break;
            case -1:
                range = ..secondIdx;
                break;
            default:
            {
                if (secondIdx == -1)
                {
                    range = (firstIdx + 1)..;
                }
                else
                {
                    range = (firstIdx + 1)..secondIdx;
                }

                break;
            }
        }

        return str[range];
    }

    /// <summary>
    /// Converts a <see cref="string"/> to an <see cref="int"/>.<br />
    /// Only works, if the input <see cref="string"/> <paramref name="str"/> contains only numbers.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be converted to a number.</param>
    /// <returns>The input <see cref="string"/> <paramref name="str"/> converted to an <see cref="int"/>.</returns>
    public static int ToInt(this string str)
    {
        return Convert.ToInt32(str);
    }

    /// <summary>
    /// Converts a <see cref="string"/> to a <see cref="long"/>.<br />
    /// Only works, if the input <see cref="string"/> <paramref name="str"/> contains only numbers.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be converted to a number.</param>
    /// <returns>The input <see cref="string"/> <paramref name="str"/> converted to a <see cref="long"/>.</returns>
    public static long ToLong(this string str)
    {
        return Convert.ToInt64(str);
    }

    /// <summary>
    /// Converts a <see cref="string"/> to a <see cref="short"/>.<br />
    /// Only works, if the input <see cref="string"/> <paramref name="str"/> contains only numbers.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be converted to a number.</param>
    /// <returns>The input <see cref="string"/> <paramref name="str"/> converted to an <see cref="short"/>.</returns>
    public static short ToShort(this string str)
    {
        return Convert.ToInt16(str);
    }

    /// <summary>
    /// Converts a <see cref="string"/> to an <see cref="uint"/>.<br />
    /// Only works, if the input <see cref="string"/> <paramref name="str"/> contains only numbers.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be converted to a number.</param>
    /// <returns>The input <see cref="string"/> <paramref name="str"/> converted to an <see cref="uint"/>.</returns>
    public static uint ToUInt(this string str)
    {
        return Convert.ToUInt32(str);
    }

    /// <summary>
    /// Converts a <see cref="string"/> to an <see cref="ulong"/>.<br />
    /// Only works, if the input <see cref="string"/> <paramref name="str"/> contains only numbers.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be converted to a number.</param>
    /// <returns>The input <see cref="string"/> <paramref name="str"/> converted to an <see cref="ulong"/>.</returns>
    public static ulong ToULong(this string str)
    {
        return Convert.ToUInt64(str);
    }

    /// <summary>
    /// Converts a <see cref="string"/> to an <see cref="ushort"/>.<br />
    /// Only works, if the input <see cref="string"/> <paramref name="str"/> contains only numbers.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be converted to a number.</param>
    /// <returns>The input <see cref="string"/> <paramref name="str"/> converted to an <see cref="ushort"/>.</returns>
    public static ushort ToUShort(this string str)
    {
        return Convert.ToUInt16(str);
    }

    public static byte ToByte(this string str)
    {
        return Convert.ToByte(str);
    }

    public static sbyte ToSByte(this string str)
    {
        return Convert.ToSByte(str);
    }

    public static float ToFloat(this string str)
    {
        return Convert.ToSingle(str);
    }

    public static double ToDouble(this string str)
    {
        return Convert.ToDouble(str);
    }

    public static decimal ToDecimal(this string str)
    {
        return Convert.ToDecimal(str);
    }

    public static bool ToBool(this string str)
    {
        return Convert.ToBoolean(str);
    }

    /// <summary>
    /// Trims all spaces in the beginning, end and middle of the <see cref="string"/> <paramref name="str"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be trimmed.</param>
    /// <returns>A trimmed <see cref="string"/>.</returns>
    public static string TrimAll(this string str)
    {
        return _multipleSpacesPattern.Replace(str.Trim(), " ");
    }

    public static StringBuilder Append(this StringBuilder builder, params string[] strings)
    {
        foreach (string s in strings)
        {
            builder.Append(s);
        }

        return builder;
    }
}
