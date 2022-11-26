using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using HLE.Http;

namespace HLE.Emojis;

/// <summary>
/// The generator I used to create the Emoji file containing every Emoji.
/// </summary>
public sealed class EmojiFileGenerator
{
    public string NamespaceName { get; set; }

    public char IndentationChar { get; set; }

    public int IndentationSize { get; set; }

    private readonly Dictionary<string, string> _illegalWords = new()
    {
        {
            "100", "Hundred"
        },
        {
            "+1", "ThumbUp"
        },
        {
            "-1", "ThumbDown"
        },
        {
            "T-rex", "TRex"
        },
        {
            "1st_place_medal", "FirstPlaceMedal"
        },
        {
            "2nd_place_medal", "SecondPlaceMedal"
        },
        {
            "3rd_place_medal", "ThirdPlaceMedal"
        },
        {
            "8ball", "EightBall"
        },
        {
            "Non-potable_water", "NonPotableWater"
        },
        {
            "1234", "OneTwoThreeFour"
        }
    };

    private JsonElement? _emojiData;
    private readonly HString _newLine = Environment.NewLine;

    public EmojiFileGenerator(string namespaceName, char indentationChar = ' ', int indentationSize = 4)
    {
        NamespaceName = namespaceName;
        IndentationChar = indentationChar;
        IndentationSize = indentationSize;
    }

    /// <summary>
    /// Generates the Emoji file.
    /// <returns>The source code of the file. Null, if the creation was unsuccessful, due to e.g. not being able to retrieve the emoji data.</returns>
    /// </summary>
    public string? Generate()
    {
        if (!_emojiData.HasValue)
        {
            HttpGet request = new("https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json");
            if (!request.IsValidJsonData)
            {
                return null;
            }

            _emojiData = request.Data;
        }

        StringBuilder builder = new();
        builder.Append($"#pragma warning disable 1591{_newLine}");
        builder.Append($"// ReSharper disable UnusedMember.Global{_newLine}");
        builder.Append($"// ReSharper disable InconsistentNaming{_newLine}");
        builder.Append((string)_newLine);
        builder.Append($"namespace {NamespaceName};{_newLine * 2}");
        builder.Append($"/// <summary>{Environment.NewLine}");
        builder.Append($"///     A class that contains (almost) every existing emoji. ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss}){Environment.NewLine}");
        builder.Append($"/// </summary>{Environment.NewLine}");
        builder.Append($"public static class Emoji{Environment.NewLine}{{{Environment.NewLine}");
        int emojiLength = _emojiData.Value.GetArrayLength();
        for (int i = 0; i < emojiLength; i++)
        {
            string? name = _emojiData.Value[i].GetProperty("aliases")[0].GetString();
            if (name is null)
            {
                continue;
            }

            Span<char> nameSpan = name.AsSpan();
            nameSpan[0] = char.ToUpper(nameSpan[0]);
            string? emoji = _emojiData.Value[i].GetProperty("emoji").GetString();
            if (emoji is null)
            {
                continue;
            }

            if (_illegalWords.TryGetValue(name, out string? replacement))
            {
                name = replacement;
            }

            builder.Append($"{new(IndentationChar, IndentationSize)}public const string {name} = \"{emoji}\";{Environment.NewLine}");
        }

        builder.Append('}');
        string content = builder.ToString();
        Span<char> chars = content.AsSpan();
        int charsLength = chars.Length;
        Span<char> result = stackalloc char[content.Length + _newLine.Length];
        int resultLength = 0;
        for (int i = 0; i < charsLength; i++)
        {
            if (chars[i] != '_')
            {
                result[resultLength++] = chars[i];
                continue;
            }

            int ip1 = i + 1;
            chars[ip1] = char.ToUpper(chars[ip1]);
        }

        for (int i = 0; i < _newLine.Length; i++)
        {
            result[resultLength++] = _newLine[i];
        }

        return new(result[..resultLength]);
    }

    public string? Generate_old()
    {
        if (!_emojiData.HasValue)
        {
            HttpGet request = new("https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json");
            if (!request.IsValidJsonData)
            {
                return null;
            }

            _emojiData = request.Data;
        }

        StringBuilder builder = new();
        builder.Append($"#pragma warning disable 1591{_newLine}");
        builder.Append($"// ReSharper disable UnusedMember.Global{_newLine}");
        builder.Append($"// ReSharper disable InconsistentNaming{_newLine}");
        builder.Append((string)_newLine);
        builder.Append($"namespace {NamespaceName};{_newLine * 2}");
        builder.Append($"/// <summary>{Environment.NewLine}");
        builder.Append($"///     A class that contains (almost) every existing emoji. ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss}){Environment.NewLine}");
        builder.Append($"/// </summary>{Environment.NewLine}");
        builder.Append($"public static class Emoji{Environment.NewLine}{{{Environment.NewLine}");
        for (int i = 0; i < _emojiData.Value.GetArrayLength(); i++)
        {
            string? name = _emojiData.Value[i].GetProperty("aliases")[0].GetString();
            if (name is null)
            {
                continue;
            }

            Span<char> nameSpan = name.AsSpan();
            nameSpan[0] = char.ToUpper(nameSpan[0]);
            string? emoji = _emojiData.Value[i].GetProperty("emoji").GetString();
            if (emoji is null)
            {
                continue;
            }

            if (_illegalWords.TryGetValue(name, out string? replacement))
            {
                name = replacement;
            }

            builder.Append($"{new(IndentationChar, IndentationSize)}public const string {name} = \"{emoji}\";{Environment.NewLine}");
        }

        builder.Append('}');
        string content = builder.ToString();
        Span<char> chars = content.AsSpan();
        int charsLength = chars.Length;
        Span<char> result = stackalloc char[content.Length + _newLine.Length];
        int resultLength = 0;
        for (int i = 0; i < charsLength; i++)
        {
            if (chars[i] != '_')
            {
                result[resultLength++] = chars[i];
                continue;
            }

            int ip1 = i + 1;
            chars[ip1] = char.ToUpper(chars[ip1]);
        }

        for (int i = 0; i < _newLine.Length; i++)
        {
            result[resultLength++] = _newLine[i];
        }

        return new(result[..resultLength]);
    }
}
