using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HLE.Text;

namespace HLE.UnitTests.Text;

public sealed class TypeFormatterTest
{
    public static TheoryData<Parameter> FormatParameters { get; } = new(
        new Parameter($"System{DefaultNamespaceSeparator}Int32[]", typeof(int[])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[][]", typeof(int[][])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[][][]", typeof(int[][][])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[{DefaultDimensionSeparator}]", typeof(int[,])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[{DefaultDimensionSeparator}{DefaultDimensionSeparator}]", typeof(int[,,])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[{DefaultDimensionSeparator}{DefaultDimensionSeparator}{DefaultDimensionSeparator}]", typeof(int[,,,])),
        new Parameter($"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Generic{DefaultNamespaceSeparator}List{DefaultOpeningDelimiter}{DefaultClosingDelimiter}", typeof(List<>)),
        new Parameter($"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Generic{DefaultNamespaceSeparator}List{DefaultOpeningDelimiter}System.Int32{DefaultClosingDelimiter}", typeof(List<int>)),
        new Parameter($"System{DefaultNamespaceSeparator}String", typeof(string)),
        new Parameter($"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Frozen{DefaultNamespaceSeparator}FrozenDictionary{DefaultOpeningDelimiter}{DefaultDimensionSeparator}{DefaultClosingDelimiter}", typeof(FrozenDictionary<,>)),
        new Parameter($"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Frozen{DefaultNamespaceSeparator}FrozenDictionary{DefaultOpeningDelimiter}System.String{DefaultGenericTypesSeparator}System.Type{DefaultClosingDelimiter}", typeof(FrozenDictionary<string, Type>)),
        new Parameter($"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Frozen{DefaultNamespaceSeparator}FrozenDictionary{DefaultOpeningDelimiter}System.String{DefaultDimensionSeparator}System.Type{DefaultClosingDelimiter}.Enumerator", typeof(FrozenDictionary<string, Type>.Enumerator)),
        new Parameter($"HLE{DefaultNamespaceSeparator}UnitTests{DefaultNamespaceSeparator}Text{DefaultNamespaceSeparator}TypeFormatterTest.Parameter", typeof(Parameter)),
        new Parameter($"HLE{DefaultNamespaceSeparator}UnitTests{DefaultNamespaceSeparator}Text{DefaultNamespaceSeparator}TypeFormatterTest.Parameter[]", typeof(Parameter[]))
    );

    private readonly TypeFormatter _formatter = new(new()
    {
        NamespaceSeparator = DefaultNamespaceSeparator,
        GenericTypesSeparator = DefaultGenericTypesSeparator,
        DimensionSeparator = DefaultDimensionSeparator,
        GenericDelimiters = s_genericDelimiters
    });

    private static readonly GenericTypeDelimiters s_genericDelimiters = new(DefaultOpeningDelimiter, DefaultClosingDelimiter);

    private const char DefaultNamespaceSeparator = '/';
    private const string DefaultGenericTypesSeparator = ", ";
    private const string DefaultDimensionSeparator = ",";
    private const string DefaultOpeningDelimiter = "[";
    private const string DefaultClosingDelimiter = "]";

    [Theory]
    [MemberData(nameof(FormatParameters))]
    public void FormatTest(Parameter parameter) => Assert.Equal(parameter.Expected, _formatter.Format(parameter.Type));

    [Fact]
    public void FormatGenericTest()
    {
        Assert.Equal($"System{DefaultNamespaceSeparator}Int32", _formatter.Format<int>());
        Assert.Equal($"System{DefaultNamespaceSeparator}String", _formatter.Format<string>());
        Assert.Equal(
            $"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Generic{DefaultNamespaceSeparator}List{DefaultOpeningDelimiter}System.Int32{DefaultClosingDelimiter}",
            _formatter.Format<List<int>>()
        );
        Assert.Equal(
            $"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Generic{DefaultNamespaceSeparator}Dictionary{DefaultOpeningDelimiter}System.Int64{DefaultGenericTypesSeparator}System.Range{DefaultClosingDelimiter}",
            _formatter.Format<Dictionary<long, Range>>()
        );
    }

    [Fact]
    public void FormatterCachesStringsTest()
    {
        string a = _formatter.Format<string[]>();
        string b = _formatter.Format<string[]>();
        Assert.Same(a, b);
    }

    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    public readonly struct Parameter(string expected, Type type)
    {
        public string Expected { get; } = expected;

        public Type Type { get; } = type;

        public override string ToString() => Type.ToString();
    }
}
