using System.IO;
using System.Reflection;
using HLE.Emojis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.EmojisTests
{
    [TestClass]
    public class EmojiFileGeneratorTest
    {
        [TestMethod]
        public void GenerateTest()
        {
            EmojiFileGenerator generator = new("Emojis");
            string content = generator.Generate();
            Assert.IsFalse(string.IsNullOrEmpty(content));
        }

        [TestMethod]
        public void ValueTest()
        {
            FieldInfo[] fields = typeof(Emoji).GetFields(BindingFlags.Public | BindingFlags.Static);
            Assert.IsTrue(fields.Length > 0);
            foreach (FieldInfo f in fields)
            {
                string? value = f.GetValue(null)?.ToString();
                Assert.IsTrue(value?.Length > 0);
            }
        }
    }
}
