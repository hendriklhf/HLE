using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HLE.SourceGenerators;

[Generator]
public sealed class AppendMethodsGenerator : ISourceGenerator
{
    private readonly List<string[]> _arguments = new();
    private readonly string[] _argumentTypes =
    {
        "scoped ReadOnlySpan<char>",
        "char"
    };

    private const int _maxAmountOfArguments = 8;

    public void Initialize(GeneratorInitializationContext context)
    {
        for (int i = 2; i <= _maxAmountOfArguments; i++)
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
        GenerateStringBuilderMethods(context);
        GenerateMessageBuilderMethods(context);
    }

    private void GenerateStringBuilderMethods(GeneratorExecutionContext context)
    {
        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine("using System;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE;").AppendLine();
        sourceBuilder.AppendLine("public ref partial struct StringBuilder");
        sourceBuilder.AppendLine("{");
        foreach (string[] argumentTypes in _arguments)
        {
            string method = CreateAppendMethod(argumentTypes);
            sourceBuilder.Append(method);
        }

        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.StringBuilder.g.cs", sourceBuilder.ToString());
    }

    private void GenerateMessageBuilderMethods(GeneratorExecutionContext context)
    {
        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine("using System;").AppendLine();
        sourceBuilder.AppendLine("namespace HLE.Twitch;").AppendLine();
        sourceBuilder.AppendLine("public partial struct MessageBuilder");
        sourceBuilder.AppendLine("{");
        foreach (string[] argumentTypes in _arguments)
        {
            string method = CreateAppendMethod(argumentTypes);
            sourceBuilder.Append(method);
        }

        sourceBuilder.AppendLine("}");
        context.AddSource("HLE.Twitch.MessageBuilder.g.cs", sourceBuilder.ToString());
    }

    private static string CreateAppendMethod(ReadOnlySpan<string> argumentTypes)
    {
        StringBuilder methodBuilder = new();
        methodBuilder.Append("public void Append(");
        for (int i = 0; i < argumentTypes.Length; i++)
        {
            if (i > 0)
            {
                methodBuilder.Append(", ");
            }

            methodBuilder.Append($"{argumentTypes[i]} arg{i}");
        }

        methodBuilder.AppendLine(")").AppendLine("{");
        for (int i = 0; i < argumentTypes.Length; i++)
        {
            methodBuilder.AppendLine($"Append(arg{i});");
        }

        methodBuilder.AppendLine("}");
        return methodBuilder.ToString();
    }
}
