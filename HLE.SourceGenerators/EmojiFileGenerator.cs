﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators;

[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
[Generator]
public sealed class EmojiFileGenerator : ISourceGenerator
{
    private byte[]? _emojiJsonBytes;
    private readonly Dictionary<string, string> _illegalVariableNames = new()
    {
        { "100", "Hundred" },
        { "+1", "ThumbUp" },
        { "-1", "ThumbDown" },
        { "T-rex", "TRex" },
        { "1st_place_medal", "FirstPlaceMedal" },
        { "2nd_place_medal", "SecondPlaceMedal" },
        { "3rd_place_medal", "ThirdPlaceMedal" },
        { "8ball", "EightBall" },
        { "Non-potable_water", "NonPotableWater" },
        { "1234", "OneTwoThreeFour" }
    };

    private static readonly TimeSpan _cacheTime = TimeSpan.FromDays(1);

    private const string _httpRequestUrl = "https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json";
    private const int _indentationSize = 4;
    private const char _indentationChar = ' ';
    private const string _cacheDirectory = "HLE.SourceGenerators.EmojiFileGenerator\\";

    public void Initialize(GeneratorInitializationContext context)
    {
        if (_emojiJsonBytes is not null)
        {
            return;
        }

        if (TryGetEmojiJsonBytesFromCache(out _emojiJsonBytes))
        {
            return;
        }

        using HttpClient httpClient = new();
        Task<byte[]> task = httpClient.GetByteArrayAsync(_httpRequestUrl);
        task.Wait();
        _emojiJsonBytes = task.Result;
        WriteBytesToCacheFile(_emojiJsonBytes);
    }

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
    private static bool TryGetEmojiJsonBytesFromCache(out byte[]? emojiJsonBytes)
    {
        string cacheDirectory = Path.GetTempPath() + _cacheDirectory;
        if (!Directory.Exists(cacheDirectory))
        {
            emojiJsonBytes = null;
            return false;
        }

        string[] files = Directory.GetFiles(cacheDirectory);
        string? emojiFilePath = files.FirstOrDefault(static f =>
        {
            string fileName = Path.GetFileName(f);
            DateTimeOffset creationTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(fileName));
            DateTimeOffset invalidationTime = creationTime + _cacheTime;
            return DateTimeOffset.UtcNow < invalidationTime;
        });

        if (emojiFilePath is null)
        {
            emojiJsonBytes = null;
            return false;
        }

        emojiJsonBytes = File.ReadAllBytes(emojiFilePath);
        return true;
    }

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
    private static void WriteBytesToCacheFile(byte[] emojiJsonBytes)
    {
        string cacheDirectory = Path.GetTempPath() + _cacheDirectory;
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        string emojiJsonPath = cacheDirectory + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        File.WriteAllBytes(emojiJsonPath, emojiJsonBytes);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (_emojiJsonBytes is null or { Length: 0 })
        {
            throw new InvalidOperationException("The HTTP request of the emojis failed.");
        }

        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine("namespace HLE.Emojis;").AppendLine();
        sourceBuilder.AppendLine("public static partial class Emoji").AppendLine("{");
        AppendEmojis(sourceBuilder);
        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.Emoji.g.cs", sourceBuilder.ToString());
    }

    private void AppendEmojis(StringBuilder sourceBuilder)
    {
        char[] indentation = new char[_indentationSize];
        indentation.AsSpan().Fill(_indentationChar);

        ReadOnlySpan<byte> emojiProperty = "emoji"u8;
        ReadOnlySpan<byte> aliasesProperty = "aliases"u8;

        JsonReaderOptions options = new()
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        Utf8JsonReader jsonReader = new(_emojiJsonBytes, options);

        char[] emojiBuffer = new char[100];
        int emojiLength = 0;
        char[] nameBuffer = new char[100];

        while (jsonReader.Read())
        {
            switch (jsonReader.TokenType)
            {
                case JsonTokenType.PropertyName when jsonReader.ValueTextEquals(emojiProperty):
                    jsonReader.Read();
                    ReadOnlySpan<byte> emojiBytes = jsonReader.ValueSpan;
                    emojiLength = Encoding.UTF8.GetChars(emojiBytes.ToArray(), 0, emojiBytes.Length, emojiBuffer, 0);
                    break;
                case JsonTokenType.PropertyName when jsonReader.ValueTextEquals(aliasesProperty):
                    jsonReader.Read();
                    jsonReader.Read();
                    ReadOnlySpan<byte> nameBytes = jsonReader.ValueSpan;
                    int nameLength = Encoding.UTF8.GetChars(nameBytes.ToArray(), 0, nameBytes.Length, nameBuffer, 0);
                    nameBuffer[0] = char.ToUpper(nameBuffer[0]);
                    CheckForIllegalName(nameBuffer, ref nameLength);

                    sourceBuilder.Append(indentation).Append("public const string ").Append(nameBuffer.AsSpan().Slice(0, nameLength).ToArray()).Append(' ');
                    sourceBuilder.Append("= \"").Append(emojiBuffer.AsSpan().Slice(0, emojiLength).ToArray()).AppendLine("\";");
                    break;
                default:
                    continue;
            }
        }
    }

    private void CheckForIllegalName(Span<char> name, ref int nameLength)
    {
        ReadOnlySpan<char> readOnlyName = name.Slice(0, nameLength);
        foreach (var illegalVariableName in _illegalVariableNames)
        {
            if (!readOnlyName.SequenceEqual(illegalVariableName.Key.AsSpan()))
            {
                continue;
            }

            illegalVariableName.Value.AsSpan().CopyTo(name);
            nameLength = illegalVariableName.Value.Length;
            return;
        }

        for (int i = 0; i < nameLength; i++)
        {
            if (name[i] != '_')
            {
                continue;
            }

            name.Slice(i + 1, nameLength).CopyTo(name.Slice(i));
            nameLength -= 1;
            name[i] = char.ToUpper(name[i]);
        }
    }
}
