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

            return list.Where(l => l is not null).Select(l => string.Join(' ', l!));
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

    public static string InsertKDots(this byte number)
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return num;
    }

    public static string InsertKDots(this sbyte number)
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this short number)
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this ushort number)
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return num;
    }

    public static string InsertKDots(this int number)
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this uint number)
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return num;
    }

    public static string InsertKDots(this long number)
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this ulong number)
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, ".");
        }

        return num;
    }
}
