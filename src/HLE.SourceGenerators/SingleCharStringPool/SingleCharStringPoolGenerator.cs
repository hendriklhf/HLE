using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HLE.SourceGenerators.SingleCharStringPool;

[Generator]
public sealed class SingleCharStringPoolGenerator : IIncrementalGenerator
{
    private const string Indentation = "    ";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EqualsValueClauseSyntax> provider = context.SyntaxProvider.CreateSyntaxProvider(IsRequiredSyntaxNode, TransformSyntaxNode);
        context.RegisterImplementationSourceOutput(provider, Execute);
    }

    private static bool IsRequiredSyntaxNode(SyntaxNode syntaxNode, CancellationToken _)
        => syntaxNode is EqualsValueClauseSyntax
        {
            Parent: VariableDeclaratorSyntax
            {
                Identifier.Text: "AmountOfCachedSingleCharStrings",
                Parent.Parent.Parent: ClassDeclarationSyntax
                {
                    Identifier.Text: "SingleCharStringPool",
                    Parent: FileScopedNamespaceDeclarationSyntax namespaceDeclaration
                }
            }
        } && namespaceDeclaration.Name.ToString() == "HLE.Text";

    private static EqualsValueClauseSyntax TransformSyntaxNode(GeneratorSyntaxContext context, CancellationToken _)
        => (EqualsValueClauseSyntax)context.Node;

    private static void Execute(SourceProductionContext context, EqualsValueClauseSyntax equalsValueClause)
    {
        string valueText = equalsValueClause.Value.ToString();
        int amountOfCachedSingleCharStrings = int.Parse(valueText);
        if (amountOfCachedSingleCharStrings < 0)
        {
            ThrowAmountLessThanZero(nameof(amountOfCachedSingleCharStrings));
        }

        string[] cachedTokenStrings = new string[amountOfCachedSingleCharStrings];
        for (ushort i = 0; i < cachedTokenStrings.Length; i++)
        {
            cachedTokenStrings[i] = $"\"\\u{i:x4}\"";
        }

        StringBuilder sourceBuilder = new(amountOfCachedSingleCharStrings * 12);
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
    private static void ThrowAmountLessThanZero(string paramName)
        => throw new ArgumentOutOfRangeException(paramName, "Amount of cached single char strings is below zero.");
}
