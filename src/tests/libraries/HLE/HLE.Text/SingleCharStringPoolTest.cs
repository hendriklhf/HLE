using HLE.TestUtilities;

namespace HLE.Text.UnitTests;

public sealed class SingleCharStringPoolTest
{
    public static TheoryData<char> GeneralPoolTestParameters { get; } = TheoryDataHelpers.CreateRange((char)0, (char)(SingleCharStringPool.AmountOfCachedSingleCharStrings * 2));

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
}
