using HLE.Strings;
using Xunit;

namespace HLE.Tests.Strings;

public sealed class SingleCharStringPoolTest
{
    public static TheoryData<char> GeneralPoolTestParameters { get; } = CreateCharParameters();

    [Fact]
    public void AmountOfCachedSingleCharStrings_NoRename_Test()
        => Assert.Equal("AmountOfCachedSingleCharStrings", nameof(SingleCharStringPool.AmountOfCachedSingleCharStrings));

    [Fact]
    public void AmountOfCachedStringsTest()
        => Assert.Equal(SingleCharStringPool.AmountOfCachedSingleCharStrings, SingleCharStringPool.GetCachedSingleCharStrings().Length);

    [Theory]
    [MemberData(nameof(GeneralPoolTestParameters))]
    public void GeneralPoolTest(char c)
    {
        Assert.Equal(c < SingleCharStringPool.AmountOfCachedSingleCharStrings, SingleCharStringPool.Contains(c));

        string str = SingleCharStringPool.GetOrAdd(c);
        Assert.Equal(1, str.Length);
        Assert.Equal(c, str[0]);

        Assert.True(SingleCharStringPool.Contains(c));
        Assert.True(SingleCharStringPool.TryGet(c, out string? test));
        Assert.Same(str, test);
    }

    private static TheoryData<char> CreateCharParameters()
    {
        TheoryData<char> data = new();
        for (int i = 0; i <= SingleCharStringPool.AmountOfCachedSingleCharStrings * 2; i++)
        {
            data.Add((char)i);
        }

        return data;
    }
}
