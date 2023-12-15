using System;
using System.Text;

namespace HLE.SourceGenerators.AppendMethods;

public sealed class ValueStringMethodBuilder(StringBuilder builder) : StringBuilderMethodBuilder(builder)
{
    private static readonly string s_sumVariable = "sum" + new Random().Next(100_000, int.MaxValue);

    private const string ThrowExceptionMethod = "ThrowNotEnoughSpaceException";
    private const string FreeBufferSizePropertyName = "FreeBufferSize";

    public override void BuildAppendReadOnlySpanMethod(ReadOnlySpan<string> arguments)
    {
        ReadOnlySpan<string> argumentNames = GetArgumentNames(arguments);

        BuildMethodHeader(arguments);
        _builder.Append(Indentation).Append('{').AppendLine();
        BuildAppendReadOnlySpanCapacityCheck(argumentNames);

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
        BuildCapacityCheck(arguments.Length.ToString());
        BuildGetReferenceCall();

        for (int i = 0; i < argumentNames.Length; i++)
        {
            BuildAppendChar(argumentNames[i], i);
        }

        _builder.Append(Indentation + Indentation).Append(LengthPropertyName).Append(" += ");
        _builder.Append(arguments.Length).Append(';').AppendLine();
        _builder.Append(Indentation).Append('}').AppendLine().AppendLine();
    }

    private void BuildAppendReadOnlySpanCapacityCheck(ReadOnlySpan<string> arguments)
    {
        _builder.Append(Indentation + Indentation);
        _builder.Append("int ").Append(s_sumVariable).Append(" = ");
        BuildSpanLengthSum(arguments);
        _builder.Append(';').AppendLine();
        BuildCapacityCheck(s_sumVariable);
    }

    private void BuildCapacityCheck(string checkValue)
    {
        _builder.Append(Indentation + Indentation);
        _builder.Append("if (").Append(FreeBufferSizePropertyName).Append(" < ").Append(checkValue);
        _builder.Append(") ").Append(ThrowExceptionMethod).AppendLine("();");
    }
}
