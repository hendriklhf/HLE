using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators.SingleCharStringPool;

[Generator]
public sealed class SingleCharStringPoolGenerator : ISourceGenerator
{
    private const string Indentation = "    ";

    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(static () => new SingleCharStringPoolReceiver());

    public void Execute(GeneratorExecutionContext context)
    {
        SingleCharStringPoolReceiver receiver = (SingleCharStringPoolReceiver)context.SyntaxReceiver!;
        if (receiver.AmountOfCachedSingleCharStrings < 0)
        {
            ThrowAmountLessThanZero(receiver);
        }

        string[] cachedTokenStrings = new string[receiver.AmountOfCachedSingleCharStrings];
        for (ushort i = 0; i < cachedTokenStrings.Length; i++)
        {
            cachedTokenStrings[i] = $"\"\\u{i:x4}\"";
        }

        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine("using System;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE.Text;").AppendLine();
        sourceBuilder.AppendLine("public static partial class SingleCharStringPool").AppendLine("{");
        sourceBuilder.Append(Indentation).AppendLine("internal static partial ReadOnlySpan<string> GetCachedSingleCharStrings() => new[]");
        sourceBuilder.Append(Indentation).Append('{');

        for (int i = 0; i < cachedTokenStrings.Length - 1; i++)
        {
            const int StringsPerLine = 8;
            if (i % StringsPerLine == 0)
            {
                sourceBuilder.AppendLine();
                sourceBuilder.Append(Indentation + Indentation);
            }

            sourceBuilder.Append(cachedTokenStrings[i]).Append(", ");
        }

        sourceBuilder.Append(cachedTokenStrings[cachedTokenStrings.Length - 1]);

        sourceBuilder.AppendLine().Append(Indentation).AppendLine("};");
        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.Text.SingleCharStringPool.g.cs", sourceBuilder.ToString());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowAmountLessThanZero(SingleCharStringPoolReceiver receiver)
        => throw new ArgumentOutOfRangeException($"{nameof(receiver)}.{nameof(receiver.AmountOfCachedSingleCharStrings)}", receiver.AmountOfCachedSingleCharStrings, "Amount of cached single char strings is below zero.");
}
