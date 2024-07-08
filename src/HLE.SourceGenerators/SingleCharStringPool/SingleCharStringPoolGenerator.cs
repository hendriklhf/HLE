using System;
using System.Diagnostics.CodeAnalysis;
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

    [SuppressMessage("Performance", "HAA0601:Value type to reference type conversion causing boxing allocation")]
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
    {
#pragma warning disable HAA0601
        object paramValue = receiver.AmountOfCachedSingleCharStrings; // boxes, as ArgumentOutOfRangeException's constructor takes an object
#pragma warning restore HAA0601

        throw new ArgumentOutOfRangeException($"{nameof(receiver)}.{nameof(receiver.AmountOfCachedSingleCharStrings)}", paramValue, "Amount of cached single char strings is below zero.");
    }
}
