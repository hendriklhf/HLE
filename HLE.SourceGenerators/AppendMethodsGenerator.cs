using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators;

[Generator]
public sealed class AppendMethodsGenerator : ISourceGenerator
{
    private readonly List<string[]> _arguments = [];
    private readonly string[] _argumentTypes = ["scoped ReadOnlySpan<char>", "char"];

    private const int MinimumAmountOfArguments = 2;
    private const int MaximumAmountOfArguments = 8;
    private const string Indentation = "    ";

    public void Initialize(GeneratorInitializationContext context)
    {
        for (int i = MinimumAmountOfArguments; i <= MaximumAmountOfArguments; i++)
        {
            for (int j = 0; j < _argumentTypes.Length; j++)
            {
                string[] types = new string[i];
                types.AsSpan().Fill(_argumentTypes[j]);
                _arguments.Add(types);
            }
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        GenerateValueStringBuilderMethods(context);
        GeneratePoolBufferStringBuilderMethods(context);
    }

    private void GenerateValueStringBuilderMethods(GeneratorExecutionContext context)
    {
        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine("using System;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE.Strings;").AppendLine();
        sourceBuilder.AppendLine("public ref partial struct ValueStringBuilder");
        sourceBuilder.AppendLine("{");
        foreach (string[] argumentTypes in _arguments)
        {
            string method = CreateAppendMethod(argumentTypes);
            sourceBuilder.Append(method);
        }

        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.Strings.ValueStringBuilder.g.cs", sourceBuilder.ToString());
    }

    private void GeneratePoolBufferStringBuilderMethods(GeneratorExecutionContext context)
    {
        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine("using System;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE.Strings;").AppendLine();
        sourceBuilder.AppendLine("public sealed partial class PooledStringBuilder");
        sourceBuilder.AppendLine("{");
        foreach (string[] argumentTypes in _arguments)
        {
            string method = CreateAppendMethod(argumentTypes);
            sourceBuilder.Append(method);
        }

        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.Strings.PooledStringBuilder.g.cs", sourceBuilder.ToString());
    }

    private static string CreateAppendMethod(ReadOnlySpan<string> argumentTypes)
    {
        const string ArgumentNamePrefix = "s";

        StringBuilder methodBuilder = new();
        methodBuilder.Append(Indentation).Append("public void Append(");
        for (int i = 0; i < argumentTypes.Length; i++)
        {
            if (i != 0)
            {
                methodBuilder.Append(", ");
            }

            methodBuilder.Append(argumentTypes[i]).Append(' ').Append(ArgumentNamePrefix).Append(i);
        }

        methodBuilder.AppendLine(")").Append(Indentation).AppendLine("{");

        for (int i = 0; i < argumentTypes.Length; i++)
        {
            string argumentName = $"{ArgumentNamePrefix}{i}";
            AppendMethodLine(methodBuilder, $"Append({argumentName});");
        }

        methodBuilder.Append(Indentation).AppendLine("}");
        return methodBuilder.ToString();
    }

    private static void AppendMethodLine(StringBuilder builder, string text)
        => builder.Append(Indentation).Append(Indentation).AppendLine(text);
}
