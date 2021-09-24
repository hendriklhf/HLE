using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.HttpRequests;
using HLE.Strings;

namespace HLE.Emojis
{
    /// <summary>
    /// The self-written generator I used to create the Emoji file containing every Emoji.
    /// </summary>
    public static class EmojiFileGenerator
    {
        /// <summary>
        /// Generates the Emoji file in the given <paramref name="path"/> and with a given namespace <paramref name="nspace"/>.<br />
        /// The <paramref name="path"/> should end with something like "\Emoji.cs".
        /// </summary>
        /// <param name="path">The path the file will be created in.</param>
        /// <param name="nspace">The namespace the class will be in.</param>
        public static void Generate(string path, string nspace)
        {
            int indentSize = 4;
            string result = string.Empty;
            List<string> charList = new();
            HttpGet request = new("https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json");
            result += $"#pragma warning disable 1591{Environment.NewLine}{Environment.NewLine}" +
                $"namespace {nspace}{Environment.NewLine}{{{Environment.NewLine}" +
                $"{Whitespace(indentSize)}/// <summary>{Environment.NewLine}" +
                $"{Whitespace(indentSize)}/// A class that contains every existing emoji. ({DateTime.Now:dd.MM.yyyy HH:mm:ss}){Environment.NewLine}" +
                $"{Whitespace(indentSize)}/// </summary>{Environment.NewLine}" +
                $"{Whitespace(indentSize)}public static class Emoji{Environment.NewLine}{Whitespace(indentSize)}{{{Environment.NewLine}";
            for (int i = 0; i <= request.Data.GetArrayLength() - 1; i++)
            {
                string name = request.Data[i].GetProperty("aliases")[0].GetString();
                name = $"{name[0]}".ToUpper() + name[1..];
                string emoji = request.Data[i].GetProperty("emoji").GetString();
                result += $"{Whitespace(indentSize << 1)}public const string {name} = \"{emoji}\";{Environment.NewLine}";
            }
            result += $"{Whitespace(indentSize)}}}{Environment.NewLine}}}";
            result.ToCharArray().ForEach(c =>
            {
                charList.Add(c.ToString());
            });
            for (int i = 0; i <= charList.Count - 1; i++)
            {
                if (charList[i] == "_")
                {
                    charList[i + 1] = charList[i + 1].ToUpper();
                    charList[i] = "";
                }
            }
            result = string.Empty;
            charList.ForEach(str =>
            {
                result += str;
            });
            GetIllegalWords().ForEach(w =>
            {
                result = result.ReplacePattern($" {Regex.Escape(w.Key)} ", $" {w.Value} ");
            });
            File.WriteAllText(path, $"{result}{Environment.NewLine}");
            GC.Collect();
        }

        private static string Whitespace(int count)
        {
            return new string(" ".ToCharArray()[0], count);
        }

        private static KeyValuePair<string, string>[] GetIllegalWords()
        {
            return new KeyValuePair<string, string>[]
            {
                new("100", "OneHundred"),
                new("+1", "ThumbUp"),
                new("-1", "ThumbDown"),
                new("T-rex", "TRex"),
                new("1stPlaceMedal", "FirstPlaceMedal"),
                new("2ndPlaceMedal", "SecondPlaceMedal"),
                new("3rdPlaceMedal", "ThirdPlaceMedal"),
                new("8ball", "EightBall"),
                new("Non-potableWater", "NonPotableWater"),
                new("1234", "OneTwoThreeFour")
            };
        }
    }
}
