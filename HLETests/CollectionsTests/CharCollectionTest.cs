using HLE.Collections;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLETests.CollectionsTests
{
    [TestClass]
    public class CharCollectionTest
    {
        [TestMethod]
        public void AlphabetTest()
        {
            CharCollection.Alphabet.ForEach(c =>
            {
                Assert.IsTrue(c.ToString().IsMatch(@"^\w$"));
            });
        }

        [TestMethod]
        public void AlphabetLowerCaseTest()
        {
            CharCollection.AlphabetLowerCase.ForEach(c =>
            {
                Assert.IsTrue(c.ToString().IsMatch(@"^\w$"));
            });
        }

        [TestMethod]
        public void AlphabetUpperCaseTest()
        {
            CharCollection.AlphabetUpperCase.ForEach(c =>
            {
                Assert.IsTrue(c.ToString().IsMatch(@"^\w$"));
            });
        }

        [TestMethod]
        public void BasicLatinCharsTest()
        {
            CharCollection.BasicLatinChars.ForEach(c =>
            {
                Assert.IsTrue(c.ToString().IsMatch(@"^\S$"));
            });
        }

        [TestMethod]
        public void CharNumbersTest()
        {
            CharCollection.CharNumbers.ForEach(c =>
            {
                Assert.IsTrue(c.ToString().IsMatch(@"^\d$"));
            });
        }

        [TestMethod]
        public void SpecialCharsTest()
        {
            CharCollection.SpecialChars.ForEach(c =>
            {
                Assert.IsTrue(c.ToString().IsMatch(@"^\S$"));
            });
        }
    }
}
