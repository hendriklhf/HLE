using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class StringBuilderTest
{
    [TestMethod]
    public void BuildTest()
    {
        StringBuilder builder = stackalloc char[50];
        builder.Append("aaa", "www");
        builder.Append('.', '_', '+');
        builder.Append("abc");
        string str = builder.ToString();
        Assert.AreEqual("aaawww._+abc", str);
        builder.Remove(6);
        str = builder.ToString();
        Assert.AreEqual("aaawww_+abc", str);
    }
}
