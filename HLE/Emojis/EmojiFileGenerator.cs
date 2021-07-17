using HLE.HttpRequests;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            HttpGet request = new("https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json");
            string result = string.Empty;
            result += $"namespace {nspace}\n{{\n{Whitespace(indentSize)}public static class Emoji\n{Whitespace(indentSize)}{{\n";
            for (int i = 0; i <= request.Data.GetArrayLength() - 1; i++)
            {
                string name = request.Data[i].GetProperty("aliases")[0].GetString();
                name = $"{name[0]}".ToUpper() + name[1..];
                string emoji = request.Data[i].GetProperty("emoji").GetString();
                result += $"{Whitespace(indentSize * 2)}public const string {name} = \"{emoji}\";\n";
            }
            result += $"{Whitespace(indentSize)}}}\n}}";
            List<string> charList = new();
            result.ToCharArray().ToList().ForEach(c =>
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
            File.WriteAllText(path, result);
        }

        private static string Whitespace(int count)
        {
            string res = string.Empty;
            for (int i = 0; i < count; i++)
            {
                res += " ";
            }
            return res;
        }
    }
}
