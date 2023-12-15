using HLE.Strings;
using Xunit;

namespace HLE.Tests.Strings;

public sealed class SingleCharStringPoolTest
{
    [Fact]
    public void AmountOfCachedSingleCharStrings_NoRename_Test()
        => Assert.Equal("AmountOfCachedSingleCharStrings", nameof(SingleCharStringPool.AmountOfCachedSingleCharStrings));

    [Fact]
    public void AmountOfCachedStringsTest()
        => Assert.Equal(SingleCharStringPool.AmountOfCachedSingleCharStrings, SingleCharStringPool.GetCachedSingleCharStrings().Length);
}
