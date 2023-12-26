using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators;

[Generator]
[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
public sealed class EmojiFileGenerator : ISourceGenerator
{
    private byte[]? _emojiJsonBytes;
    private readonly Dictionary<string, string> _illegalEmojiNameReplacements = new(12)
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
        { "1234", "OneTwoThreeFour" },
        { "icecream", "SoftIceCream" },
        { "ice_cream", "IceCream" }
    };

    private static readonly TimeSpan s_cacheTime = TimeSpan.FromDays(1);

    private const string HttpRequestUrl = "https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json";
    private const string Indentation = "    ";
    private const string CacheDirectory = "HLE.SourceGenerators.EmojiFileGenerator\\";

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
        Task<byte[]> task = httpClient.GetByteArrayAsync(HttpRequestUrl);
        task.Wait();
        _emojiJsonBytes = task.Result;
        WriteBytesToCacheFile(_emojiJsonBytes);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (_emojiJsonBytes is not { Length: not 0 })
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

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
    private static bool TryGetEmojiJsonBytesFromCache(out byte[]? emojiJsonBytes)
    {
        string cacheDirectory = Path.GetTempPath() + CacheDirectory;
        if (!Directory.Exists(cacheDirectory))
        {
            emojiJsonBytes = null;
            return false;
        }

        string[] files = Directory.GetFiles(cacheDirectory);
        string? emojiFilePath = Array.Find(files, static f =>
        {
            string fileName = Path.GetFileName(f);
            DateTimeOffset creationTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(fileName));
            DateTimeOffset invalidationTime = creationTime + s_cacheTime;
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
        string cacheDirectory = Path.GetTempPath() + CacheDirectory;
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        string emojiJsonPath = cacheDirectory + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        File.WriteAllBytes(emojiJsonPath, emojiJsonBytes);
    }

    private void AppendEmojis(StringBuilder sourceBuilder)
    {
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

                    sourceBuilder.Append(Indentation).Append("public const string ").Append(nameBuffer.AsSpan().Slice(0, nameLength).ToArray()).Append(' ');
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
        foreach (KeyValuePair<string, string> illegalVariableName in _illegalEmojiNameReplacements)
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
            nameLength--;
            name[i] = char.ToUpper(name[i]);
        }
    }
}
