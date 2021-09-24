using System.Collections.Generic;
using HLE.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLETests.EnumsTest
{
    [TestClass]
    public class EnumHelperTest
    {
        [DataRow(0)]
        [DataRow(2)]
        [TestMethod]
        public void ToArrayTest(int idx)
        {
            TestDataProvider.TestEnum[] enumArr = typeof(TestDataProvider.TestEnum).ToArray<TestDataProvider.TestEnum>();
            Assert.AreEqual(enumArr[idx], (TestDataProvider.TestEnum)idx);
        }

        [DataRow(0)]
        [DataRow(2)]
        [TestMethod]
        public void ToListTest(int idx)
        {
            List<TestDataProvider.TestEnum> enumList = typeof(TestDataProvider.TestEnum).ToList<TestDataProvider.TestEnum>();
            Assert.AreEqual(enumList[idx], (TestDataProvider.TestEnum)idx);
        }
    }
}
