using System;
using System.Collections.Generic;
using System.Linq;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.CollectionsTests
{
    [TestClass]
    public class NumberCollectionTest
    {
        [TestMethod]
        public void NumbersTest()
        {
            Assert.AreEqual(10, NumberCollection.Numbers.Count());
        }

        [TestMethod]
        public void CreateByteTest()
        {
            byte[] arr = NumberCollection.Create(byte.MinValue).ToArray();
            Assert.AreEqual(byte.MinValue, arr[0]);
            Assert.AreEqual(byte.MaxValue, arr[^1]);
        }
        
        [TestMethod]
        public void CreateSByteTest()
        {
            sbyte[] arr = NumberCollection.Create(sbyte.MinValue).ToArray();
            Assert.AreEqual(sbyte.MinValue, arr[0]);
            Assert.AreEqual(sbyte.MaxValue, arr[^1]);
        }
        
        [TestMethod]
        public void CreateShortTest()
        {
            short[] arr = NumberCollection.Create(short.MinValue).ToArray();
            Assert.AreEqual(short.MinValue, arr[0]);
            Assert.AreEqual(short.MaxValue, arr[^1]);
        }
        
        [TestMethod]
        public void CreateUShortTest()
        {
            ushort[] arr = NumberCollection.Create(ushort.MinValue).ToArray();
            Assert.AreEqual(ushort.MinValue, arr[0]);
            Assert.AreEqual(ushort.MaxValue, arr[^1]);
        }
        
        //[TestMethod]
        public void CreateIntTest()
        {
            int[] arr = NumberCollection.Create(int.MinValue).ToArray();
            Assert.AreEqual(int.MinValue, arr[0]);
            Assert.AreEqual(int.MaxValue, arr[^1]);
        }
        
        //[TestMethod]
        public void CreateUIntTest()
        {
            uint[] arr = NumberCollection.Create(uint.MinValue).ToArray();
            Assert.AreEqual(uint.MinValue, arr[0]);
            Assert.AreEqual(uint.MaxValue, arr[^1]);
        }
        
        //[TestMethod]
        public void CreateLongTest()
        {
            long[] arr = NumberCollection.Create(long.MinValue).ToArray();
            Assert.AreEqual(long.MinValue, arr[0]);
            Assert.AreEqual(long.MaxValue, arr[^1]);
        }
        
        //[TestMethod]
        public void CreateULongTest()
        {
            ulong[] arr = NumberCollection.Create(ulong.MinValue).ToArray();
            Assert.AreEqual(ulong.MinValue, arr[0]);
            Assert.AreEqual(ulong.MaxValue, arr[^1]);
        }
    }
}
