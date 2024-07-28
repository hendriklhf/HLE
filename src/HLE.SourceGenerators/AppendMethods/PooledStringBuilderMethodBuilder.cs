using System;
using System.Text;

namespace HLE.SourceGenerators.AppendMethods;

public sealed class PooledStringBuilderMethodBuilder(StringBuilder builder) : StringBuilderMethodBuilder(builder)
{
    private const string EnsureCapacityMethodName = "EnsureCapacity";

    public override void BuildAppendReadOnlySpanMethod(ReadOnlySpan<string> arguments)
    {
        ReadOnlySpan<string> argumentNames = GetArgumentNames(arguments);

        BuildMethodHeader(arguments);
        _builder.Append(Indentation).Append('{').AppendLine();
        BuildEnsureCapacityReadOnlySpanCall(argumentNames);

        for (int i = 0; i < argumentNames.Length; i++)
        {
            BuildCopyIntoBuilderCall(argumentNames[i]);
        }

        _builder.Append(Indentation).Append('}').AppendLine().AppendLine();
    }

    public override void BuildAppendCharMethod(ReadOnlySpan<string> arguments)
    {
        ReadOnlySpan<string> argumentNames = GetArgumentNames(arguments);

        BuildMethodHeader(arguments);
        _builder.Append(Indentation).Append('{').AppendLine();
        BuildEnsureCapacityCharCall(arguments.Length);
        BuildGetReferenceCall();

        for (int i = 0; i < argumentNames.Length; i++)
        {
            BuildAppendChar(argumentNames[i], i);
        }

        _builder.Append(Indentation + Indentation).Append(LengthPropertyName).Append(" += ");
        _builder.Append(arguments.Length).Append(';').AppendLine();
        _builder.Append(Indentation).Append('}').AppendLine().AppendLine();
    }

    private void BuildEnsureCapacityCharCall(int charCount)
    {
        _builder.Append(Indentation + Indentation).Append(EnsureCapacityMethodName).Append('(');
        _builder.Append(LengthPropertyName).Append(" + ");
        _builder.Append(charCount);
        _builder.AppendLine(");");
    }

    private void BuildEnsureCapacityReadOnlySpanCall(ReadOnlySpan<string> argumentNames)
    {
        _builder.Append(Indentation + Indentation).Append(EnsureCapacityMethodName).Append('(');
        _builder.Append(LengthPropertyName).Append(" + ");
        BuildSpanLengthSum(argumentNames);
        _builder.AppendLine(");");
    }
}
