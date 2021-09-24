using System.IO;
using HLE.Emojis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLETests.EmojisTests
{
    [TestClass]
    public class EmojiFileGeneratorTest
    {
        [TestMethod]
        public void GenerateTest()
        {
            EmojiFileGenerator.Generate(@".\Emoji.cs", "Emojis");
            Assert.IsFalse(string.IsNullOrEmpty(File.ReadAllText(@".\Emoji.cs")));
        }
    }
}
