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
    private const string _indentation = "    ";

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
        StringBuilder methodBuilder = new();
        methodBuilder.Append(_indentation).Append("public void Append(");
        for (int i = 0; i < argumentTypes.Length; i++)
        {
            if (i > 0)
            {
                methodBuilder.Append(", ");
            }

            methodBuilder.Append(argumentTypes[i]).Append(" arg").Append(i);
        }

        methodBuilder.AppendLine(")").Append(_indentation).AppendLine("{");
        for (int i = 0; i < argumentTypes.Length; i++)
        {
            methodBuilder.Append(_indentation + _indentation).Append("Append(arg").Append(i).AppendLine(");");
        }

        methodBuilder.Append(_indentation).AppendLine("}");
        return methodBuilder.ToString();
    }
}
