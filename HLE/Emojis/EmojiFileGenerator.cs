using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

    private byte[]? _emojiJsonBytes;

    private const string _publicConstString = "public const string";
    private const string _equalSignSpaceQuotation = "= \"";
    private const string _quotationSemicolon = "\";";

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
        if (_emojiJsonBytes is null)
        {
            try
            {
                using HttpClient httpClient = new();
                Task<byte[]> task = httpClient.GetByteArrayAsync("https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json");
                task.Wait();
                byte[] bytes = task.Result;
                _emojiJsonBytes = bytes;
            }
            catch (Exception)
            {
                return null;
            }
        }

        StringBuilder builder = new(stackalloc char[100000]);
        builder.Append($"#pragma warning disable 1591{Environment.NewLine}");
        builder.Append($"// ReSharper disable UnusedMember.Global{Environment.NewLine}");
        builder.Append($"// ReSharper disable InconsistentNaming{Environment.NewLine}");
        builder.Append(Environment.NewLine);
        builder.Append($"namespace {NamespaceName};{Environment.NewLine + Environment.NewLine}");
        builder.Append($"/// <summary>{Environment.NewLine}");
        builder.Append($"///     A class that contains (almost) every existing emoji. (generated {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss}){Environment.NewLine}");
        builder.Append($"/// </summary>{Environment.NewLine}");
        builder.Append($"public static class Emoji{Environment.NewLine}{{{Environment.NewLine}");

        Span<char> indentation = stackalloc char[IndentationSize];
        indentation.Fill(IndentationChar);

        ReadOnlySpan<byte> emojiProperty = "emoji"u8;
        ReadOnlySpan<byte> aliasesProperty = "aliases"u8;

        JsonReaderOptions options = new()
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        Utf8JsonReader jsonReader = new(_emojiJsonBytes, options);

        Span<char> emoji = stackalloc char[100];
        int emojiLength = 0;
        Span<char> name = stackalloc char[100];

        while (jsonReader.Read())
        {
            switch (jsonReader.TokenType)
            {
                case JsonTokenType.PropertyName when jsonReader.ValueTextEquals(emojiProperty):
                    jsonReader.Read();
                    ReadOnlySpan<byte> emojiBytes = jsonReader.ValueSpan;
                    emojiLength = Encoding.UTF8.GetChars(emojiBytes, emoji);
                    break;
                case JsonTokenType.PropertyName when jsonReader.ValueTextEquals(aliasesProperty):
                    jsonReader.Read();
                    jsonReader.Read();
                    ReadOnlySpan<byte> nameBytes = jsonReader.ValueSpan;
                    int nameLength = Encoding.UTF8.GetChars(nameBytes, name);
                    name[0] = char.ToUpper(name[0]);
                    CheckForIllegalName(name, ref nameLength);

                    builder.Append(indentation, _publicConstString, StringHelper.Whitespace, name[..nameLength], StringHelper.Whitespace);
                    builder.Append(_equalSignSpaceQuotation, emoji[..emojiLength], _quotationSemicolon, Environment.NewLine);
                    break;
                default:
                    continue;
            }
        }

        builder.Append('}');
        builder.Append(Environment.NewLine);
        return builder.ToString();
    }

    private void CheckForIllegalName(Span<char> name, ref int nameLength)
    {
        ReadOnlySpan<char> readOnlyName = name[..nameLength];
        foreach (var illegalWord in _illegalWords)
        {
            if (!readOnlyName.Equals(illegalWord.Key, StringComparison.Ordinal))
            {
                continue;
            }

            illegalWord.Value.CopyTo(name);
            nameLength = illegalWord.Value.Length;
            return;
        }

        for (int i = 0; i < nameLength; i++)
        {
            if (name[i] != '_')
            {
                continue;
            }

            name[(i + 1)..nameLength].CopyTo(name[i..]);
            nameLength--;
            name[i] = char.ToUpper(name[i]);
        }
    }
}
