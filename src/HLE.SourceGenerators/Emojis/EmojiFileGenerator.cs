using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators.Emojis;

[Generator]
[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
public sealed class EmojiFileGenerator : ISourceGenerator
{
    private Emoji[]? _emojis;

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

    private const string HttpRequestUrl = "https://raw.githubusercontent.com/github/gemoji/master/db/emoji.json";
    private const string Indentation = "    ";
    private const string CacheDirectory = "HLE/SourceGenerators/EmojiFileGenerator";

    // ReSharper disable once AsyncVoidMethod
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    public async void Initialize(GeneratorInitializationContext _)
    {
        using Stream emojiJsonBytes = await GetEmojiJsonBytesAsync();
        _emojis = await JsonSerializer.DeserializeAsync<Emoji[]>(emojiJsonBytes);
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

    public void Execute(GeneratorExecutionContext context)
    {
        StringBuilder sourceBuilder = new(65536);
        sourceBuilder.AppendLine("namespace HLE.Emojis;").AppendLine();
        sourceBuilder.AppendLine("public static partial class Emoji").AppendLine("{");
        AppendEmojis(sourceBuilder, _emojis);
        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.Emojis.Emoji.g.cs", sourceBuilder.ToString());
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

    private static void AppendEmojis(StringBuilder sourceBuilder, ReadOnlySpan<Emoji> emojis)
    {
        for (int i = 0; i < emojis.Length; i++)
        {
            Emoji emoji = emojis[i];
            sourceBuilder.Append(Indentation).Append("public const string ").Append(NormalizeName(emoji.Name)).Append(" = ");
            sourceBuilder.Append('"').Append(emoji.Value).AppendLine("\";");
        }
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
