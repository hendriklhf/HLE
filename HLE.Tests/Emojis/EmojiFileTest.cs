using System.Reflection;
using HLE.Emojis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Emojis;

[TestClass]
public class EmojiFileTest
{
    [TestMethod]
    public void ValueTest()
    {
        FieldInfo[] fields = typeof(Emoji).GetFields(BindingFlags.Public | BindingFlags.Static);
        Assert.IsTrue(fields.Length > 0);
        foreach (FieldInfo f in fields)
        {
            object? value = f.GetValue(null);
            Assert.IsTrue(value is string { Length: > 0 });
        }
    }
}
