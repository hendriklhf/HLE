using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators.AppendMethods;

[Generator]
public sealed class AppendMethodsGenerator : ISourceGenerator
{
    private readonly List<ImmutableArray<string>> _readOnlySpanArguments = [];
    private readonly List<ImmutableArray<string>> _charArguments = [];

    private const int DefaultStringBuilderCapacity = 32384;
    private const int MinimumAmountOfArguments = 2;
    private const int MaximumAmountOfArguments = 8;
    private const string ReadOnlySpanArgumentNamePrefix = "s";
    private const string CharArgumentNamePrefix = "c";
    private const string ReadOnlySpanArgumentType = "scoped global::System.ReadOnlySpan<char>";
    private const string CharArgumentType = "char";
    private const string StringBuilderNamespace = "HLE.Strings";
    private const string ValueStringBuilderClassName = "ValueStringBuilder";
    private const string PooledStringBuilderClassName = "PooledStringBuilder";

    public void Initialize(GeneratorInitializationContext _)
    {
        for (int argumentCount = MinimumAmountOfArguments; argumentCount <= MaximumAmountOfArguments; argumentCount++)
        {
            ImmutableArray<string> readOnlySpanArguments = Enumerable.Range(0, argumentCount)
                .Select(static i => $"{ReadOnlySpanArgumentType} {ReadOnlySpanArgumentNamePrefix}{i}")
                .ToImmutableArray();

            _readOnlySpanArguments.Add(readOnlySpanArguments);

            ImmutableArray<string> charArguments = Enumerable.Range(0, argumentCount)
                .Select(static i => $"{CharArgumentType} {CharArgumentNamePrefix}{i}")
                .ToImmutableArray();
            _charArguments.Add(charArguments);
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        GenerateValueStringBuilderMethods(context);
        GeneratePooledStringBuilderMethods(context);
    }

    private void GenerateValueStringBuilderMethods(GeneratorExecutionContext context)
    {
        StringBuilder sourceBuilder = new(DefaultStringBuilderCapacity);
        sourceBuilder.AppendLine($"namespace {StringBuilderNamespace};").AppendLine();
        sourceBuilder.AppendLine($"public ref partial struct {ValueStringBuilderClassName}");
        sourceBuilder.AppendLine("{");

        PooledStringBuilderMethodBuilder methodBuilder = new(sourceBuilder);

        foreach (ImmutableArray<string> arguments in _readOnlySpanArguments)
        {
            methodBuilder.BuildAppendReadOnlySpanMethod(arguments.AsSpan());
        }

        foreach (ImmutableArray<string> arguments in _charArguments)
        {
            methodBuilder.BuildAppendCharMethod(arguments.AsSpan());
        }

        sourceBuilder.AppendLine("}");
        context.AddSource($"{StringBuilderNamespace}.{ValueStringBuilderClassName}.g.cs", sourceBuilder.ToString());
    }

    private void GeneratePooledStringBuilderMethods(GeneratorExecutionContext context)
    {
        StringBuilder sourceBuilder = new(DefaultStringBuilderCapacity);
        sourceBuilder.AppendLine($"namespace {StringBuilderNamespace};").AppendLine();
        sourceBuilder.AppendLine($"public sealed partial class {PooledStringBuilderClassName}");
        sourceBuilder.AppendLine("{");

        PooledStringBuilderMethodBuilder methodBuilder = new(sourceBuilder);

        foreach (ImmutableArray<string> arguments in _readOnlySpanArguments)
        {
            methodBuilder.BuildAppendReadOnlySpanMethod(arguments.AsSpan());
        }

        foreach (ImmutableArray<string> arguments in _charArguments)
        {
            methodBuilder.BuildAppendCharMethod(arguments.AsSpan());
        }

        sourceBuilder.AppendLine("}");
        context.AddSource($"{StringBuilderNamespace}.{PooledStringBuilderClassName}.g.cs", sourceBuilder.ToString());
    }
}
