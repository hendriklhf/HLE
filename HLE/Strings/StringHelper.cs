using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HLE.Strings
{
    /// <summary>
    /// A class to help with any kind of <see cref="string"/>.
    /// </summary>
    public static class StringHelper
    {
        private static readonly Regex _spacePattern = new(@"\s", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
        private static readonly Regex _multipleSpacesPattern = new(@"\s+", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

        /// <summary>
        /// This char moves the chars to the right of this char to the left in Twitch chat.
        /// </summary>
        public const char ZeroWidthChar = '�';

        /// <summary>
        /// This char is not visible in Twitch chat.
        /// </summary>
        public const char InvisibleChar = '⠀';

        /// <summary>
        /// Decodes a <see cref="byte"/> <see cref="Array"/> to a UTF-8 <see cref="string"/>.
        /// </summary>
        /// <param name="bytes">The <see cref="byte"/> array that will be decoded.</param>
        /// <returns>Returns the decoded array as a <see cref="string"/>.</returns>
        public static string Decode(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Encodes a UTF-8 <see cref="string"/> to a <see cref="byte"/> array.
        /// </summary>
        /// <param name="str">The <see cref="string"/> that will be encoded.</param>
        /// <returns>Returns the encoded <see cref="string"/> as a <see cref="byte"/> array.</returns>
        public static byte[] Encode(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Matches the input <see cref="string"/> <paramref name="str"/> for the given Regex <paramref name="pattern"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> that will be matches with the <paramref name="pattern"/>.</param>
        /// <param name="pattern">The Regex <paramref name="pattern"/> for the match.</param>
        /// <returns>Returns true, if <paramref name="str"/> matches at least once, false otherwise.</returns>
        public static bool IsMatch(this string str, string pattern)
        {
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(str);
        }

        /// <summary>
        /// Matches the input <see cref="string"/> <paramref name="str"/> for the given Regex <paramref name="pattern"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> that will be matches with the <paramref name="pattern"/>.</param>
        /// <param name="pattern">The Regex <paramref name="pattern"/> for the match.</param>
        /// <returns>Returns the <see cref="string"/> that matches the Regex <paramref name="pattern"/>.</returns>
        public static string Match(this string str, string pattern)
        {
            return new Regex(pattern, RegexOptions.IgnoreCase).Match(str).Value;
        }

        /// <summary>
        /// Matches the input <see cref="string"/> <paramref name="str"/> for the given Regex <paramref name="pattern"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> that will be matches with the <paramref name="pattern"/>.</param>
        /// <param name="pattern">The Regex <paramref name="pattern"/> for the match.</param>
        /// <returns>Returns a <see cref="List{String}"/> that contains every match of the input <see cref="string"/> <paramref name="str"/>.</returns>
        public static IEnumerable<string> Matches(this string str, string pattern)
        {
            return new Regex(pattern, RegexOptions.IgnoreCase).Matches(str).Select(m => m.Value);
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

        public static string Remove(this string str, char c)
        {
            return str.Replace(c.ToString(), "");
        }

        /// <summary>
        /// Replaces the Regex match of the <paramref name="pattern"/> in the input <see cref="string"/> <paramref name="str"/> with the <paramref name="replacement"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> in which the match will be replaced.</param>
        /// <param name="pattern">The Regex pattern for the match.</param>
        /// <param name="replacement">The <paramref name="replacement"/> that will be put into the input <see cref="string"/> <paramref name="str"/>.</param>
        /// <returns>The input <see cref="string"/> <paramref name="str"/> with the matching part replaced by the <paramref name="replacement"/>.</returns>
        public static string ReplacePattern(this string str, string pattern, string replacement)
        {
            return Regex.Replace(str, pattern, replacement, RegexOptions.IgnoreCase);
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

        public static bool HasSpaces(this string str)
        {
            return _spacePattern.IsMatch(str);
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

        /// <summary>
        /// Trims all spaces in the beginning, end and middle of the <see cref="string"/> <paramref name="str"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> that will be trimmed.</param>
        /// <returns>A trimmed <see cref="string"/>.</returns>
        public static string TrimAll(this string str)
        {
            return _multipleSpacesPattern.Replace(str.Trim(), " ");
        }

        /// <summary>
        /// Returns a <see cref="string"/> that contains the given amount <paramref name="count"/> of spaces.
        /// </summary>
        /// <param name="count">The amount of spaces the <see cref="string"/> will contain.</param>
        /// <returns>The <see cref="string"/> filled with whitespaces.</returns>
        public static string Whitespace(int count)
        {
            return new string(' ', count);
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
}
