using HLE.Emojis;
using System.IO;
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
            Assert.IsTrue(string.IsNullOrEmpty(File.ReadAllText(@".\Emojis.cs")));
        }
    }
}
