using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class StringBuilderTest
{
    [TestMethod]
    public void BuildTest()
    {
        ValueStringBuilder builder = stackalloc char[50];
        builder.Append("aaa", "www");
        builder.Append('.', '_', '+');
        builder.Append("abc");
        builder.Append(23);
        string str = builder.ToString();
        Assert.AreEqual("aaawww._+abc23", str);
        builder.Remove(6);
        str = builder.ToString();
        Assert.AreEqual("aaawww_+abc23", str);
    }
}
