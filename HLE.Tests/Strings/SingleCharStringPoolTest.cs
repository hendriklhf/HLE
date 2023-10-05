using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class SingleCharStringPoolTest
{
    [TestMethod]
    public void AmountOfCachedSingleCharStrings_NoRename_Test() =>
        Assert.AreEqual("AmountOfCachedSingleCharStrings", nameof(SingleCharStringPool.AmountOfCachedSingleCharStrings));

    [TestMethod]
    public void AmountOfCachedStringsTest()
        => Assert.AreEqual(SingleCharStringPool.AmountOfCachedSingleCharStrings, SingleCharStringPool.GetCachedSingleCharStrings().Length);
}
