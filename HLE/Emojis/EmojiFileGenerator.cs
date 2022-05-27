using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Http;

namespace HLE.Emojis;

/// <summary>
/// The generator I used to create the Emoji file containing every Emoji.
/// </summary>
public class EmojiFileGenerator
{
    public string FilePath { get; set; }

    public string NamespaceName { get; set; }

    public char IndentationChar { get; set; }

    public int IndentationSize { get; set; }

    private readonly (string Original, string Replacement)[] _illegalWords =
    {
        ("100", "OneHundred"),
        ("+1", "ThumbUp"),
        ("-1", "ThumbDown"),
        ("T-rex", "TRex"),
        ("1stPlaceMedal", "FirstPlaceMedal"),
        ("2ndPlaceMedal", "SecondPlaceMedal"),
        ("3rdPlaceMedal", "ThirdPlaceMedal"),
        ("8ball", "EightBall"),
        ("Non-potableWater", "NonPotableWater"),
        ("1234", "OneTwoThreeFour")
    };

    private JsonElement? _emojiData;

    public EmojiFileGenerator(string filePath, string namespaceName, char indentationChar = ' ', int indentationSize = 4)
    {
        FilePath = filePath;
        NamespaceName = namespaceName;
        IndentationChar = indentationChar;
        IndentationSize = indentationSize;
    }

    /// <summary>
    /// Generates the Emoji file in the given <see cref="FilePath"/> and with a given namespace <see cref="NamespaceName"/>.<br />
    /// </summary>
    public void Generate()
    {
        if (!_emojiData.HasValue)
        {
            HttpGet request = new("https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json");
            _emojiData = request.Data;
        }

        StringBuilder builder = new();
        builder.Append($"#pragma warning disable 1591{Environment.NewLine}{Environment.NewLine}");
        builder.Append($"namespace {NamespaceName}{Environment.NewLine}{{{Environment.NewLine}");
        builder.Append($"{new string(IndentationChar, IndentationSize)}/// <summary>{Environment.NewLine}");
        builder.Append($"{new string(IndentationChar, IndentationSize)}/// A class that contains every existing emoji. ({DateTime.Now:dd.MM.yyyy HH:mm:ss}){Environment.NewLine}");
        builder.Append($"{new string(IndentationChar, IndentationSize)}/// </summary>{Environment.NewLine}");
        builder.Append($"{new string(IndentationChar, IndentationSize)}public static class Emoji{Environment.NewLine}{new string(IndentationChar, IndentationSize)}{{{Environment.NewLine}");
        for (int i = 0; i < _emojiData.Value.GetArrayLength(); i++)
        {
            string? name = _emojiData.Value[i].GetProperty("aliases")[0].GetString();
            if (name is null)
            {
                continue;
            }

            name = char.ToUpper(name[0]) + name[1..];
            string? emoji = _emojiData.Value[i].GetProperty("emoji").GetString();
            if (emoji is null)
            {
                continue;
            }

            builder.Append($"{new string(IndentationChar, IndentationSize << 1)}public const string {name} = \"{emoji}\";{Environment.NewLine}");
        }

        builder.Append($"{new string(IndentationChar, IndentationSize)}}}{Environment.NewLine}}}");
        char[] charList = builder.ToString().ToCharArray();
        for (int i = 0; i < charList.Length; i++)
        {
            if (charList[i] != '_')
            {
                continue;
            }

            charList[i + 1] = char.ToUpper(charList[i + 1]);
        }

        builder = new(charList.Where(c => c != '_').ConcatToString());
        builder.Append(Environment.NewLine);
        string result = builder.ToString();
        _illegalWords.ForEach(w =>
        {
            string pattern = $@"\s{Regex.Escape(w.Original)}\s";
            string replacement = $" {w.Replacement} ";
            result = Regex.Replace(result, pattern, replacement);
        });
        File.WriteAllText(FilePath, result);
    }
}
