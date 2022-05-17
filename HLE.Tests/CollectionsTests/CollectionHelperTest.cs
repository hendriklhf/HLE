using System.Collections.Generic;
using System.Linq;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.CollectionsTests
{
    [TestClass]
    public class CollectionHelperTest
    {
        [TestMethod]
        public void RandomTest()
        {
            string[] arr =
            {
                "1",
                "2",
                "3",
                "4",
                "5"
            };
            for (int i = 0; i <= 50; i++)
            {
                Assert.IsTrue(arr.Contains(arr.Random()));
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

            arr.ForEach(_ => idx++);
            Assert.AreEqual(arraySize, idx);
            Assert.AreEqual(0, arr[0]);

            for (int i = 0; i < arr.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(default, arr[i]);
                        break;
                    default:
                        Assert.AreNotEqual(default, arr[i]);
                        break;
                }
            }
        }

        [TestMethod]
        public void IsNullOrEmptyTest()
        {
            int[]? arrNull = null;
            Assert.IsTrue(arrNull.IsNullOrEmpty());
            int[] arr = new int[10];
            Assert.IsFalse(arr.IsNullOrEmpty());
            int[] arrFull =
            {
                1,
                2,
                3
            };
            Assert.IsFalse(arrFull.IsNullOrEmpty());
            List<int>? listNull = null;
            Assert.IsTrue(listNull.IsNullOrEmpty());
            List<int> listEmpty = new();
            Assert.IsTrue(listEmpty.IsNullOrEmpty());
        }

        [TestMethod]
        public void JoinToStringTest()
        {
            string[] arr =
            {
                "a",
                "b",
                "c"
            };
            Assert.AreEqual("a b c", arr.JoinToString(' '));
        }

        [TestMethod]
        public void ConcatToStringTest()
        {
            string[] arr =
            {
                "a",
                "b",
                "c"
            };

            Assert.AreEqual("abc", arr.ConcatToString());
        }

        [TestMethod]
        public void SwapTest()
        {
            int[] arr =
            {
                1,
                2,
                3
            };

            arr = arr.Swap(0, 2).ToArray();
            Assert.AreEqual(1, arr[2]);
            Assert.AreEqual(3, arr[0]);
        }

        [TestMethod]
        public void ReplaceTest()
        {
            int[] arr =
            {
                1,
                2,
                3,
                2,
                5,
                2,
                2,
                2,
                3,
                3
            };
            arr = arr.Replace(i => i == 2, 4).ToArray();
            Assert.AreEqual(5, arr.Count(i => i == 4));
        }

        [TestMethod]
        public void SelectEachTest()
        {
            int[][] arrArr =
            {
                new[]
                {
                    0,
                    1,
                    2,
                    3,
                    4
                },
                new[]
                {
                    5,
                    6,
                    7,
                    8,
                    9
                },
                new[]
                {
                    10,
                    11,
                    12,
                    13,
                    14
                }
            };

            int[] arr = arrArr.SelectEach().ToArray();
            Assert.AreEqual(15, arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(i, arr[i]);
            }
        }

        [TestMethod]
        public void SplitTest()
        {
            string[] arr =
            {
                ".",
                ".",
                "A",
                "B",
                "C",
                ".",
                "D",
                ".",
                ".",
                "E",
                "F",
                "G",
                "H",
                ".",
                "I",
                "J",
                ".",
                "."
            };

            var split = arr.Split(".");
            Assert.AreEqual(4, split.Length);
            byte[] lengths =
            {
                3,
                1,
                4,
                2
            };
            for (int i = 0; i < split.Length; i++)
            {
                Assert.AreEqual(lengths[i], split[i].Length);
            }
        }

        [TestMethod]
        public void WherePTest()
        {
            bool Longer3(string s) => s.Length > 3;
            bool Shorter5(string s) => s.Length < 5;

            string[] arr =
            {
                "aaaaaaaa",
                "aaa",
                "aaaa",
                "aaaaa",
                "a",
                "aaaa",
                "aaaaaaaaa",
                "aaaa"
            };

            arr = arr.WhereP(Longer3, Shorter5).ToArray();
            Assert.AreEqual(3, arr.Length);
            Assert.IsTrue(arr.All(s => s.Length == 4));
        }

        [TestMethod]
        public void RandomWordTest()
        {
            char[] arr =
            {
                'A',
                'B',
                'C'
            };

            string word = arr.RandomWord(5);
            Assert.AreEqual(5, word.Length);
        }
    }
}
