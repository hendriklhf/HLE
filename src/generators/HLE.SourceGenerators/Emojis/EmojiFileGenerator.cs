using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators.Emojis;

[Generator]
[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
[SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider")]
public sealed class EmojiFileGenerator : IIncrementalGenerator
{
    private static readonly Dictionary<string, string> s_illegalEmojiNameReplacements = new()
    {
        { "1stPlaceMedal", "FirstPlaceMedal" },
        { "2ndPlaceMedal", "SecondPlaceMedal" },
        { "3rdPlaceMedal", "ThirdPlaceMedal" },
        { "Japanese“acceptable”Button", "JapaneseAcceptableButton" },
        { "Japanese“application”Button", "JapaneseApplicationButton" },
        { "Japanese“bargain”Button", "JapaneseBargainButton" },
        { "Japanese“congratulations”Button", "JapaneseCongratulationsButton" },
        { "Japanese“discount”Button", "JapaneseDiscountButton" },
        { "Japanese“freeOfCharge”Button", "JapaneseFreeOfChargeButton" },
        { "Japanese“here”Button", "JapaneseHereButton" },
        { "Japanese“monthlyAmount”Button", "JapaneseMonthlyAmountButton" },
        { "Japanese“noVacancy”Button", "JapaneseNoVacancyButton" },
        { "Japanese“notFreeOfCharge”Button", "JapaneseNotFreeOfChargeButton" },
        { "Japanese“openForBusiness”Button", "JapaneseOpenForBusinessButton" },
        { "Japanese“passingGrade”Button", "JapanesePassingGradeButton" },
        { "Japanese“prohibited”Button", "JapaneseProhibitedButton" },
        { "Japanese“reserved”Button", "JapaneseReservedButton" },
        { "Japanese“secret”Button", "JapaneseSecretButton" },
        { "Japanese“serviceCharge”Button", "JapaneseServiceChargeButton" },
        { "Japanese“vacancy”Button", "JapaneseVacancyButton" },
        { "Keycap0", "KeycapZero" },
        { "Keycap1", "KeycapOne" },
        { "Keycap10", "KeycapTen" },
        { "Keycap2", "KeycapTwo" },
        { "Keycap3", "KeycapThree" },
        { "Keycap4", "KeycapFour" },
        { "Keycap5", "KeycapFive" },
        { "Keycap6", "KeycapSix" },
        { "Keycap7", "KeycapSeven" },
        { "Keycap8", "KeycapEight" },
        { "Keycap9", "KeycapNine" }
    };

    private static readonly Regex s_namePattern = new("\"cldr\":\\s*\"(.+)\"", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex s_emojiPattern = new("\"glyph\":\\s*\"(.+)\"", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private const string Indentation = "    ";

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterPostInitializationOutput(Execute);

    private static void Execute(IncrementalGeneratorPostInitializationContext context)
    {
        EmojiModel[] emojis = ParseEmojis();

        StringBuilder sourceBuilder = new(ushort.MaxValue * 8);
        sourceBuilder.AppendLine("using System.Collections.Frozen;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE.Text;").AppendLine();
        sourceBuilder.AppendLine("public static partial class Emoji").AppendLine("{");

        Array.Sort(emojis, static (x, y) => string.CompareOrdinal(x.Name, y.Name));
        CreateEmojiConstants(sourceBuilder, emojis);

        CreateEmojiByNameSet(sourceBuilder, emojis);

        Array.Sort(emojis, static (x, y) => string.CompareOrdinal(x.Value, y.Value));
        CreateEmojiSet(sourceBuilder, emojis);

        sourceBuilder.AppendLine("}");
        string source = sourceBuilder.ToString();

        context.AddSource("HLE.Text.Emoji.g.cs", source);
    }

    private static EmojiModel[] ParseEmojis()
    {
        string[] resourceNames = typeof(EmojiFileGenerator).Assembly.GetManifestResourceNames();
        List<EmojiModel> emojis = new(resourceNames.Length);

        foreach (string resourceName in resourceNames)
        {
            if (!resourceName.EndsWith("metadata.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            using Stream stream = typeof(EmojiFileGenerator).Assembly.GetManifestResourceStream(resourceName)!;
            using StreamReader reader = new(stream);
            string content = reader.ReadToEnd();

            string emojiName = s_namePattern.Match(content).Groups[1].Value;
            emojiName = NormalizeName(emojiName);

            string emojiValue = s_emojiPattern.Match(content).Groups[1].Value;

            EmojiModel emoji = new(emojiName, emojiValue);
            emojis.Add(emoji);
        }

        return emojis.ToArray();
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
        sourceBuilder.AppendLine().Append(Indentation)
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

        sourceBuilder.AppendLine()
            .Append(Indentation)
            .AppendLine("private static partial global::System.Collections.Frozen.FrozenDictionary<string, string> EmojisByName => s_emojisByName;");
    }

    private static void CreateEmojiSet(StringBuilder sourceBuilder, ReadOnlySpan<EmojiModel> emojis)
    {
        sourceBuilder.AppendLine().Append(Indentation)
            .AppendLine("private static readonly global::System.Collections.Frozen.FrozenSet<string> s_allEmojis = new string[]")
            .Append(Indentation).AppendLine("{");

        for (int i = 0; i < emojis.Length; i++)
        {
            sourceBuilder.Append(Indentation, 2)
                .Append("\"").Append(emojis[i].Value).Append("\"");
            string trail = i == emojis.Length - 1 ? string.Empty : ",";
            sourceBuilder.AppendLine(trail);
        }

        sourceBuilder.Append(Indentation).AppendLine("}.ToFrozenSet(global::System.StringComparer.Ordinal);");

        sourceBuilder.AppendLine()
            .Append(Indentation)
            .AppendLine("private static partial global::System.Collections.Frozen.FrozenSet<string> AllEmojis => s_allEmojis;");
    }

    private static string NormalizeName(string name)
    {
        bool neededNormalization = false;
        Span<char> buffer = stackalloc char[name.Length];
        name.AsSpan().CopyTo(buffer);
        char firstChar = buffer[0];
        if (char.IsLower(firstChar))
        {
            buffer[0] = char.ToUpper(firstChar);
            neededNormalization = true;
        }

        if (buffer.EndsWith("(blood type)"))
        {
            buffer = buffer.Slice(0, buffer.Length - 13);
        }

        for (int i = 1; i < buffer.Length; i++)
        {
            char c = buffer[i];

            if (c is ' ' or '_' or '-' or ':' or '!' or '.' or '’')
            {
                if (buffer[i + 1] == ' ')
                {
                    buffer.Slice(i + 2).CopyTo(buffer.Slice(i));
                    buffer = buffer.Slice(0, buffer.Length - 2);
                }
                else
                {
                    buffer.Slice(i + 1).CopyTo(buffer.Slice(i));
                    buffer = buffer.Slice(0, buffer.Length - 1);
                }

                buffer[i] = char.ToUpper(buffer[i]);
                neededNormalization = true;
                continue;
            }
        }

        string result = neededNormalization ? buffer.ToString() : name;

        if (s_illegalEmojiNameReplacements.TryGetValue(result, out string? replacement))
        {
            result = replacement;
        }

        return result;
    }
}
