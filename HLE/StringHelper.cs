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
    /// Is invisible in Twitch chat.
    /// </summary>
    public const char InvisibleChar = '\uFFFD';

    /// <summary>
    /// Creates a invisible block in Twitch chat.
    /// </summary>
    public const char InvisibleBlockChar = '\u2800';

    /// <summary>
    /// Can be placed inside a username, which not mention the user.
    /// </summary>
    public const string AntipingChar = "\uDB40\uDC00";

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

        string[] split = str.Split();
        List<List<string>> list = new();
        int listIdx = 0;
        int sum = 0;
        foreach (string s in split)
        {
            if (list.Count < listIdx + 1)
            {
                list.Add(new());
            }

            bool exceedsMaxCharCount = sum + list[listIdx].Count + s.Length > charCount;
            if (!exceedsMaxCharCount)
            {
                list[listIdx].Add(s);
                sum += s.Length;
            }
            else
            {
                if (sum == 0)
                {
                    list[listIdx].Add(s);
                }
                else
                {
                    list.Add(new());
                    list[++listIdx].Add(s);
                }

                listIdx++;
                sum = 0;
            }
        }

        return list.Select(l => string.Join(' ', l));
    }

    public static string TakeBetween(this string str, char firstChar, char secondChar)
    {
        int firstIdx = str.IndexOf(firstChar);
        int secondIdx = str.IndexOf(secondChar, firstIdx + 1);
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

    public static string InsertKDots(this byte number, char kchar = '.')
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return num;
    }

    public static string InsertKDots(this sbyte number, char kchar = '.')
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this short number, char kchar = '.')
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this ushort number, char kchar = '.')
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return num;
    }

    public static string InsertKDots(this int number, char kchar = '.')
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this uint number, char kchar = '.')
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return num;
    }

    public static string InsertKDots(this long number, char kchar = '.')
    {
        bool negative = number < 0;
        string num = negative ? number.ToString()[1..] : number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return negative ? '-' + num : num;
    }

    public static string InsertKDots(this ulong number, char kchar = '.')
    {
        string num = number.ToString();
        if (num.Length < 4)
        {
            return num;
        }

        string c = kchar.ToString();
        for (int i = num.Length - 3; i > 0; i -= 3)
        {
            num = num.Insert(i, c);
        }

        return num;
    }
}
