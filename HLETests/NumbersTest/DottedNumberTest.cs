using HLE.Numbers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLETests.NumbersTest
{
    [TestClass]
    public class DottedNumberTest
    {
        [TestMethod]
        public void CreateTest()
        {
            long[] arr = { 123456789, 1234, 123, -123456 };
            string[] expectation = { "123.456.789", "1.234", "123", "-123.456" };
            for (int i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(new DottedNumber(arr[i]).Number, expectation[i]);
            }
        }
    }
}
