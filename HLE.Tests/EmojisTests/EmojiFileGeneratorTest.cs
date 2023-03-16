using System.Reflection;
using HLE.Emojis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.EmojisTests;

[TestClass]
public class EmojiFileGeneratorTest
{
    [TestMethod]
    public void ValueTest()
    {
        FieldInfo[] fields = typeof(Emoji).GetFields(BindingFlags.Public | BindingFlags.Static);
        Assert.IsTrue(fields.Length > 0);
        foreach (FieldInfo f in fields)
        {
            string value = (string)f.GetValue(null)!;
            Assert.IsTrue(value.Length > 0);
        }
    }
}
