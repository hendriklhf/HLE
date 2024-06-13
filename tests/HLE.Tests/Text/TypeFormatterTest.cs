using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HLE.Text;
using Xunit;

namespace HLE.Tests.Text;

public sealed class TypeFormatterTest
{
    public static TheoryData<Parameter> FormatParameters { get; } = new(
        new Parameter($"System{DefaultNamespaceSeparator}Int32[]", typeof(int[])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[][]", typeof(int[][])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[][][]", typeof(int[][][])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[,]", typeof(int[,])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[,,]", typeof(int[,,])),
        new Parameter($"System{DefaultNamespaceSeparator}Int32[,,,]", typeof(int[,,,])),
        new Parameter($"System{DefaultNamespaceSeparator}String", typeof(string)),
        new Parameter($"HLE{DefaultNamespaceSeparator}Tests{DefaultNamespaceSeparator}Text{DefaultNamespaceSeparator}TypeFormatterTest.Parameter", typeof(Parameter)),
        new Parameter($"HLE{DefaultNamespaceSeparator}Tests{DefaultNamespaceSeparator}Text{DefaultNamespaceSeparator}TypeFormatterTest.Parameter[]", typeof(Parameter[])),
        new Parameter($"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Frozen{DefaultNamespaceSeparator}FrozenDictionary{DefaultOpeningDelimiter}System.String{DefaultGenericTypesSeparator}System.Type{DefaultClosingDelimiter}", typeof(FrozenDictionary<string, Type>)),
        new Parameter($"System{DefaultNamespaceSeparator}Collections{DefaultNamespaceSeparator}Frozen{DefaultNamespaceSeparator}FrozenDictionary.Enumerator{DefaultOpeningDelimiter}System.String{DefaultGenericTypesSeparator}System.Type{DefaultClosingDelimiter}", typeof(FrozenDictionary<string, Type>.Enumerator))
    );

    private readonly TypeFormatter _formatter = new(new()
    {
        NamespaceSeparator = DefaultNamespaceSeparator,
        GenericTypesSeparator = DefaultGenericTypesSeparator,
        GenericDelimiters = s_genericDelimiters
    });

    private static readonly GenericTypeDelimiters s_genericDelimiters = new(DefaultOpeningDelimiter, DefaultClosingDelimiter);

    private const char DefaultNamespaceSeparator = '/';
    private const string DefaultGenericTypesSeparator = ", ";
    private const string DefaultOpeningDelimiter = "{";
    private const string DefaultClosingDelimiter = "}";

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

        public override string ToString() => $"\"{Expected}\", {Type}";
    }
}
