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
            EmojiFileGenerator generator = new(@".\Emoji.cs", "Emojis");
            generator.Generate();
            Assert.IsFalse(string.IsNullOrEmpty(File.ReadAllText(@".\Emoji.cs")));
        }

        [TestMethod]
        public void ValueTest()
        {
            FieldInfo[] fields = typeof(Emoji).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo f in fields)
            {
                string? value = f.GetValue(null)?.ToString();
                Assert.IsTrue(value?.Length > 0);
            }
        }
    }
}
