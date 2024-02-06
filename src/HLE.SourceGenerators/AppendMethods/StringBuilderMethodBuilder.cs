using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace HLE.SourceGenerators.AppendMethods;

public abstract class StringBuilderMethodBuilder(StringBuilder builder)
{
    private protected readonly StringBuilder _builder = builder;

    private protected const string AppendMethodName = "Append";
    private protected const string Indentation = "    ";
    private protected const string HleMemoryNamespace = "HLE.Memory";
    private protected const string CopyWorkerTypeName = "CopyWorker";
    private protected const string CopyWorkerGenericParameter = "<char>";
    private protected const string CopyMethodName = "Copy";
    private protected const string FreeBufferSpanPropertyName = "FreeBufferSpan";
    private protected const string SystemRuntimeCompilerServicesNamespace = "System.Runtime.CompilerServices";
    private protected const string SystemRuntimeInteropServicesNamespace = "System.Runtime.InteropServices";
    private protected const string UnsafeTypeName = nameof(Unsafe);
    private protected const string AddMethodName = nameof(Unsafe.Add);
    private protected const string MemoryMarshalTypeName = nameof(MemoryMarshal);
    private protected const string GetReferenceMethodName = nameof(MemoryMarshal.GetReference);
    private protected const string LengthPropertyName = "Length";
    private protected const string CapacityPropertyName = "Capacity";
    private protected const string BufferReferenceVariableName = "reference";

    private const string ArgumentSeparator = ", ";

    public abstract void BuildAppendReadOnlySpanMethod(ReadOnlySpan<string> arguments);

    public abstract void BuildAppendCharMethod(ReadOnlySpan<string> arguments);

    private protected void BuildMethodHeader(ReadOnlySpan<string> arguments)
    {
        _builder.Append(Indentation);
        _builder.Append("public void ").Append(AppendMethodName).Append('(');
        _builder.AppendJoin(ArgumentSeparator, arguments);
        _builder.Append(')').AppendLine();
    }

    private protected void BuildCopyIntoBuilderCall(string argument)
    {
        _builder.Append(Indentation + Indentation);
        _builder.Append("global::").Append(HleMemoryNamespace).Append('.').Append(CopyWorkerTypeName);
        _builder.Append(CopyWorkerGenericParameter).Append('.').Append(CopyMethodName).Append('(');
        _builder.Append(argument).Append(", ").Append(FreeBufferSpanPropertyName).AppendLine(");");
        _builder.Append(Indentation + Indentation);
        _builder.Append(LengthPropertyName).Append(" += ").Append(argument).Append('.').Append(LengthPropertyName).Append(';').AppendLine();
    }

    private protected void BuildGetReferenceCall()
    {
        _builder.Append(Indentation + Indentation);
        _builder.Append("ref char ").Append(BufferReferenceVariableName).Append(" = ref ");
        _builder.Append("global::").Append(SystemRuntimeInteropServicesNamespace).Append('.');
        _builder.Append(MemoryMarshalTypeName).Append('.').Append(GetReferenceMethodName).Append('(');
        _builder.Append(FreeBufferSpanPropertyName).AppendLine(");");
    }

    private protected void BuildSpanLengthSum(ReadOnlySpan<string> argumentNames)
    {
        int length = argumentNames.Length - 1;

        for (int i = 0; i < length; i++)
        {
            _builder.Append(argumentNames[i]).Append('.').Append(LengthPropertyName);
            _builder.Append(" + ");
        }

        _builder.Append(argumentNames[argumentNames.Length - 1]).Append('.').Append(LengthPropertyName);
    }

    private protected void BuildAppendChar(string argument, int charIndex)
    {
        _builder.Append(Indentation + Indentation);
        _builder.Append("global::").Append(SystemRuntimeCompilerServicesNamespace).Append('.');
        _builder.Append(UnsafeTypeName).Append('.').Append(AddMethodName).Append("(ref ").Append(BufferReferenceVariableName).Append(", ");
        _builder.Append(charIndex).Append(") = ").Append(argument).Append(';').AppendLine();
    }

    private protected static ReadOnlySpan<string> GetArgumentNames(ReadOnlySpan<string> arguments)
    {
        string[] result = new string[arguments.Length];
        for (int i = 0; i < arguments.Length; i++)
        {
            string argument = arguments[i];
            int lastWhitespace = argument.LastIndexOf(' ');
            result[i] = argument.Substring(lastWhitespace + 1);
        }

        return result;
    }
}
