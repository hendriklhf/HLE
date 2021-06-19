using Sterbehilfe.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sterbehilfe.Strings
{
    /// <summary>
    /// A class to help with any kind of <see cref="string"/>.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Converts the <paramref name="input"/> to string by appending all elements beginning at index 0.
        /// </summary>
        /// <param name="input">The <see cref="string"/> array that will be converted to a <see cref="string"/>.</param>
        /// <returns>Returns the <paramref name="input"/> as a <see cref="string"/>.</returns>
        public static string ArrayToString(this string[] input)
        {
            string result = string.Empty;
            input.ToList().ForEach(str =>
            {
                result += $"{str} ";
            });
            return result.Trim();
        }

        public static string Decode(this byte[] bytes)
        {
#warning add encoding enum
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] Encode(this string str)
        {
#warning add encoding enum
            return Encoding.UTF8.GetBytes(str);
        }

        public static string EscapeChars(this string str)
        {
            new List<string>() { "\0", "\b", "\n", "\r", "\t", "'", "\"", "\\", }.ForEach(c =>
            {
                str = str.Replace(c, "");
            });
            return str;
        }

        public static bool IsMatch(this string str, string pattern)
        {
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(str);
        }

        public static string Match(this string str, string pattern)
        {
            return new Regex(pattern, RegexOptions.IgnoreCase).Match(str).Value;
        }

        public static string Remove(this string str, string stringToRemove)
        {
            return str.Replace(stringToRemove, "");
        }

        public static string RemoveHashtag(this string str)
        {
            return str.Replace("#", "");
        }

        public static string ReplaceChatterinoChar(this string str)
        {
            return str.Replace(Resources.ChatterinoChar, "");
        }
        public static string ReplacePattern(this string str, string pattern, string replacement)
        {
            return Regex.Replace(str, pattern, replacement, RegexOptions.IgnoreCase);
        }

        public static string ReplaceSpaces(this string str)
        {
            return str.ReplacePattern(@"\s+", " ");
        }

        public static List<string> Split(this string str, int charCount)
        {
            if (str.Length > charCount)
            {
                List<string> result = new();
                result.Add(str[..charCount]);
                str = str[charCount..];
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
                return new() { str };
            }
        }

        public static int ToInt(this string str)
        {
            return Convert.ToInt32(str);
        }

        public static long ToLong(this string str)
        {
            return Convert.ToInt64(str);
        }

        public static short ToShort(this string str)
        {
            return Convert.ToInt16(str);
        }

        public static uint ToUInt(this string str)
        {
            return Convert.ToUInt32(str);
        }

        public static ulong ToULong(this string str)
        {
            return Convert.ToUInt64(str);
        }

        public static ushort ToUShort(this string str)
        {
            return Convert.ToUInt16(str);
        }
    }
}