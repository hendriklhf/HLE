using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace HLE.IO.UnitTests;

public sealed class PathHelpersTest
{
    private static readonly char s_namespaceSeperator = Path.DirectorySeparatorChar;
    private const string DefaultOpeningDelimiter = "{";
    private const string DefaultClosingDelimiter = "}";
    private const string DefaultGenericTypesSeparator = ", ";
    private const string DefaultDimensionSeparator = ",";

    public static TheoryData<Parameter> FormatParameters { get; } = new(
        new Parameter($"System{s_namespaceSeperator}Int32[]", typeof(int[])),
        new Parameter($"System{s_namespaceSeperator}Int32[][]", typeof(int[][])),
        new Parameter($"System{s_namespaceSeperator}Int32[][][]", typeof(int[][][])),
        new Parameter($"System{s_namespaceSeperator}Int32[{DefaultDimensionSeparator}]", typeof(int[,])),
        new Parameter($"System{s_namespaceSeperator}Int32[{DefaultDimensionSeparator}{DefaultDimensionSeparator}]", typeof(int[,,])),
        new Parameter($"System{s_namespaceSeperator}Int32[{DefaultDimensionSeparator}{DefaultDimensionSeparator}{DefaultDimensionSeparator}]", typeof(int[,,,])),
        new Parameter($"System{s_namespaceSeperator}Collections{s_namespaceSeperator}Generic{s_namespaceSeperator}List{DefaultOpeningDelimiter}{DefaultClosingDelimiter}", typeof(List<>)),
        new Parameter($"System{s_namespaceSeperator}Collections{s_namespaceSeperator}Generic{s_namespaceSeperator}List{DefaultOpeningDelimiter}System.Int32{DefaultClosingDelimiter}", typeof(List<int>)),
        new Parameter($"System{s_namespaceSeperator}String", typeof(string)),
        new Parameter($"System{s_namespaceSeperator}Collections{s_namespaceSeperator}Frozen{s_namespaceSeperator}FrozenDictionary{DefaultOpeningDelimiter}{DefaultDimensionSeparator}{DefaultClosingDelimiter}", typeof(FrozenDictionary<,>)),
        new Parameter($"System{s_namespaceSeperator}Collections{s_namespaceSeperator}Frozen{s_namespaceSeperator}FrozenDictionary{DefaultOpeningDelimiter}System.String{DefaultGenericTypesSeparator}System.Type{DefaultClosingDelimiter}", typeof(FrozenDictionary<string, Type>)),
        new Parameter($"System{s_namespaceSeperator}Collections{s_namespaceSeperator}Frozen{s_namespaceSeperator}FrozenDictionary{DefaultOpeningDelimiter}System.String{DefaultGenericTypesSeparator}System.Type{DefaultClosingDelimiter}.Enumerator", typeof(FrozenDictionary<string, Type>.Enumerator)),
        new Parameter($"HLE{s_namespaceSeperator}UnitTests{s_namespaceSeperator}IO{s_namespaceSeperator}{nameof(PathHelpersTest)}.{nameof(Parameter)}", typeof(Parameter)),
        new Parameter($"HLE{s_namespaceSeperator}UnitTests{s_namespaceSeperator}IO{s_namespaceSeperator}{nameof(PathHelpersTest)}.{nameof(Parameter)}[]", typeof(Parameter[]))
    );

    private static readonly char[] s_invalidPathChars = Path.GetInvalidPathChars();

    [Theory]
    [MemberData(nameof(FormatParameters))]
    public void TypeNameToPath_NonGeneric(Parameter parameter)
    {
        string path = PathHelpers.TypeNameToPath(parameter.Type);
        Assert.Equal(parameter.Expected, path);
        Assert.False(path.AsSpan().ContainsAny(s_invalidPathChars));
    }

    [Theory]
    [MemberData(nameof(FormatParameters))]
    public void TypeNameToPath_CachesResult(Parameter parameter)
    {
        string a = PathHelpers.TypeNameToPath(parameter.Type);
        string b = PathHelpers.TypeNameToPath(parameter.Type);
        Assert.Same(a, b);
    }

    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    public readonly struct Parameter(string expected, Type type)
    {
        public string Expected { get; } = expected;

        public Type Type { get; } = type;

        public override string ToString() => $"\"{Expected}\", {Type}";
    }
}
