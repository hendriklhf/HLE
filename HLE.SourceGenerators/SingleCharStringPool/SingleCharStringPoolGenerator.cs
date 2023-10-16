using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators.SingleCharStringPool;

[Generator]
public sealed class SingleCharStringPoolGenerator : ISourceGenerator
{
    private const string _indentation = "    ";

    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(static () => new SingleCharStringPoolReceiver());

    public void Execute(GeneratorExecutionContext context)
    {
        SingleCharStringPoolReceiver receiver = (SingleCharStringPoolReceiver)context.SyntaxReceiver!;
        if (receiver.AmountOfCachedSingleCharStrings < 0)
        {
            throw new ArgumentOutOfRangeException(null, receiver.AmountOfCachedSingleCharStrings, "Amount of cached single char strings is below zero.");
        }

        string[] cachedTokenStrings = new string[receiver.AmountOfCachedSingleCharStrings];
        for (ushort i = 0; i < cachedTokenStrings.Length; i++)
        {
            cachedTokenStrings[i] = $"\"\\u{i:x4}\"";
        }

        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine("using System;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE.Strings;").AppendLine();
        sourceBuilder.AppendLine("public static partial class SingleCharStringPool").AppendLine("{");
        sourceBuilder.Append(_indentation).AppendLine("private static readonly string[] _cachedSingleCharStrings =");
        sourceBuilder.Append(_indentation).Append('{');

        for (int i = 0; i < cachedTokenStrings.Length - 1; i++)
        {
            if (i % 8 == 0)
            {
                sourceBuilder.AppendLine();
                sourceBuilder.Append(_indentation + _indentation);
            }

            sourceBuilder.Append(cachedTokenStrings[i] + ", ");
        }

        sourceBuilder.Append(cachedTokenStrings[cachedTokenStrings.Length - 1]);

        sourceBuilder.AppendLine().Append(_indentation).AppendLine("};").AppendLine();
        sourceBuilder.Append(_indentation).AppendLine("internal static partial ReadOnlySpan<string> GetCachedSingleCharStrings() => _cachedSingleCharStrings;");
        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.Strings.ShortStringCache.g.cs", sourceBuilder.ToString());
    }
}
