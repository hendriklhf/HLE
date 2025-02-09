using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators.Emojis;

[Generator]
[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
[SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider")]
public sealed class EmojiFileGenerator : IIncrementalGenerator
{
    private static readonly Dictionary<string, string> s_illegalEmojiNameReplacements = new()
    {
        { "+1", "ThumbsUp" },
        { "-1", "ThumbsDown" },
        { "1st_place_medal", "FirstPlaceMedal" },
        { "2nd_place_medal", "SecondPlaceMedal" },
        { "3rd_place_medal", "ThirdPlaceMedal" },
        { "8ball", "EightBall" },
        { "1234", "OneTwoThreeFour" },
        { "100", "OneHundred" },
        { "icecream", "SoftIceCream" },
        { "ice_cream", "IceCream" }
    };

    private static readonly TimeSpan s_cacheTime = TimeSpan.FromDays(1);
    private static readonly Regex s_emojiPattern = new("\"emoji\":\\s*\"(.*)\"", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex s_namePattern = new("\"aliases\":\\s*\\[\\s*\"(.*)\"(\\s*,\\s*\".*\")*\\s*\\]", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private const string HttpRequestUrl = "https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json";
    private const string Indentation = "    ";
    private const string CacheDirectory = "HLE/SourceGenerators/EmojiFileGenerator";

    // ReSharper disable once AsyncVoidMethod
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    public async void Initialize(IncrementalGeneratorInitializationContext context)
    {
        using Stream emojiJsonStream = await GetEmojiJsonBytesAsync();
        EmojiModel[] emojis = await ParseEmojisAsync(emojiJsonStream);
        context.RegisterImplementationSourceOutput(context.CompilationProvider, (context, _) => Execute(context, emojis));
    }

    private static async Task<EmojiModel[]> ParseEmojisAsync(Stream emojiJsonStream)
    {
        using StreamReader reader = new(emojiJsonStream);
        string json = await reader.ReadToEndAsync();
        List<string> emojis = new();
        foreach (Match match in s_emojiPattern.Matches(json))
        {
            string value = match.Groups[1].Value;
            emojis.Add(value);
        }

        List<string> names = new(emojis.Count);
        foreach (Match match in s_namePattern.Matches(json))
        {
            string value = match.Groups[1].Value;
            names.Add(value);
        }

        if (emojis.Count != names.Count)
        {
            throw new InvalidOperationException($"Emoji count: {emojis.Count} != Names count: {names.Count}");
        }

        EmojiModel[] emojiModels = new EmojiModel[emojis.Count];
        for (int i = 0; i < emojis.Count; i++)
        {
            emojiModels[i] = new(NormalizeName(names[i]), emojis[i]);
        }

        return emojiModels;
    }

    private static ValueTask<Stream> GetEmojiJsonBytesAsync()
    {
        return TryGetEmojiJsonBytesFromCache(out Stream? emojiJsonBytes) ? new(emojiJsonBytes!) : GetEmojiJsonBytesCoreAsync();

        static async ValueTask<Stream> GetEmojiJsonBytesCoreAsync()
        {
            using HttpClient httpClient = new();
            Stream? emojiJsonBytes = await httpClient.GetStreamAsync(HttpRequestUrl);
            await WriteBytesToCacheFileAsync(emojiJsonBytes);
            return emojiJsonBytes;
        }
    }

    private static void Execute(SourceProductionContext context, EmojiModel[] emojis)
    {
        StringBuilder sourceBuilder = new(ushort.MaxValue * 8);
        sourceBuilder.AppendLine("using System.Collections.Frozen;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE.Text;").AppendLine();
        sourceBuilder.AppendLine("public static partial class Emoji").AppendLine("{");
        CreateEmojiConstants(sourceBuilder, emojis);
        CreateEmojiByNameSet(sourceBuilder, emojis);
        sourceBuilder.AppendLine("}");
        string source = sourceBuilder.ToString();
        context.AddSource("HLE.Text.Emoji.g.cs", source);
    }

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
    private static bool TryGetEmojiJsonBytesFromCache(out Stream? emojiJsonBytes)
    {
        string cacheDirectory = Path.Combine(Path.GetTempPath(), CacheDirectory);
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

        emojiJsonBytes = File.OpenRead(emojiFilePath);
        return true;
    }

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
    private static async Task WriteBytesToCacheFileAsync(Stream emojiJsonBytes)
    {
        string cacheDirectory = Path.Combine(Path.GetTempPath(), CacheDirectory);
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        string emojiJsonPath = Path.Combine(cacheDirectory, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
        using FileStream jsonFile = File.OpenWrite(emojiJsonPath);
        await emojiJsonBytes.CopyToAsync(jsonFile);
    }

    private static void CreateEmojiConstants(StringBuilder sourceBuilder, ReadOnlySpan<EmojiModel> emojis)
    {
        for (int i = 0; i < emojis.Length; i++)
        {
            EmojiModel emoji = emojis[i];
            sourceBuilder.Append(Indentation).Append("/// <summary> ").Append(emoji.Value).AppendLine(" </summary>");
            sourceBuilder.Append(Indentation).Append("public const string ").Append(emoji.Name).Append(" = ");
            sourceBuilder.Append('"').Append(emoji.Value).AppendLine("\";");
        }
    }

    private static void CreateEmojiByNameSet(StringBuilder sourceBuilder, ReadOnlySpan<EmojiModel> emojis)
    {
        sourceBuilder.AppendLine()
            .Append(Indentation)
            .AppendLine("private static partial global::System.Collections.Frozen.FrozenDictionary<string, string> EmojisByName => s_emojisByName;")
            .AppendLine();

        sourceBuilder.Append(Indentation)
            .Append("private static readonly global::System.Collections.Frozen.FrozenDictionary<string, string> s_emojisByName = new global::System.Collections.Generic.KeyValuePair<string, string>[")
            .Append(emojis.Length).AppendLine("]").Append(Indentation).AppendLine("{");

        for (int i = 0; i < emojis.Length; i++)
        {
            sourceBuilder.Append(Indentation, 2).Append("new(nameof(").Append(emojis[i].Name)
                .Append("), ").Append(emojis[i].Name).Append(")");

            string trail = i == emojis.Length - 1 ? string.Empty : ",";
            sourceBuilder.AppendLine(trail);
        }

        sourceBuilder.Append(Indentation).AppendLine("}.ToFrozenDictionary(global::System.StringComparer.OrdinalIgnoreCase);");
    }

    private static string NormalizeName(string name)
    {
        if (s_illegalEmojiNameReplacements.TryGetValue(name, out string replacement))
        {
            return replacement;
        }

        bool neededNormalization = false;
        Span<char> buffer = stackalloc char[name.Length];
        name.AsSpan().CopyTo(buffer);
        char firstChar = buffer[0];
        if (char.IsLower(firstChar))
        {
            buffer[0] = char.ToUpper(firstChar);
            neededNormalization = true;
        }

        for (int i = 1; i < buffer.Length; i++)
        {
            char c = buffer[i];
            if (c is not '_' and not ' ' and not '-')
            {
                continue;
            }

            buffer.Slice(i + 1).CopyTo(buffer.Slice(i));
            buffer = buffer.Slice(0, buffer.Length - 1);
            buffer[i] = char.ToUpper(buffer[i]);
            neededNormalization = true;
        }

        return neededNormalization ? buffer.ToString() : name;
    }
}
