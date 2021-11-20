using System.Collections.Generic;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLETests.CollectionsTests
{
    [TestClass]
    public class CollectionHelperTest
    {
        [TestMethod]
        public void RandomTest()
        {
            string[] arr = { "1", "2", "3", "4", "5" };
            for (int i = 0; i <= 50; i++)
            {
                Assert.IsFalse(string.IsNullOrEmpty(arr.Random()));
            }
        }

        [TestMethod]
        public void ForEachTest()
        {
            int idx = 0;
            int arraySize = 50;
            int[] arr = new int[arraySize];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i;
            }
            arr.ForEach(a => idx++);
            Assert.IsTrue(idx == arraySize);
        }

        [TestMethod]
        public void IsNullOrEmptyTest()
        {
            int[] arrNull = null;
            Assert.IsTrue(arrNull.IsNullOrEmpty());
            int[] arr = new int[10];
            Assert.IsFalse(arr.IsNullOrEmpty());
            int[] arrFull = { 1, 2, 3 };
            Assert.IsFalse(arrFull.IsNullOrEmpty());
            List<int> listNull = null;
            Assert.IsTrue(listNull.IsNullOrEmpty());
            List<int> listEmpty = new();
            Assert.IsTrue(listEmpty.IsNullOrEmpty());
        }

        [TestMethod]
        public void ToSequenceTest()
        {
            string[] arr = { "a", "b", "c" };
            Assert.AreEqual(arr.JoinToString(' '), "a b c");
        }

        [DataRow('-')]
        [TestMethod]
        public void ToSequenceTest(char c)
        {
            string[] arr = { "a", "b", "c" };
            Assert.AreEqual(arr.JoinToString('-'), "a-b-c");
        }
    }
}
