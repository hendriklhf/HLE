using System;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class UnmanagedStringTest
{
    [TestMethod]
    public void CreateFromLengthTest()
    {
        for (int length = 0; length <= 10_000; length *= 10)
        {
            using UnmanagedString unmanagedString = UnmanagedString.Create(length);

            Assert.AreEqual(length, unmanagedString.Length);
            Assert.AreEqual(length, unmanagedString.String.Length);
            Assert.AreEqual(length, unmanagedString.AsSpan().Length);

            Assert.IsTrue(unmanagedString.AsSpan().SequenceEqual(unmanagedString.String));

            if (length == 0)
            {
                length = 1;
            }
        }
    }

    [TestMethod]
    public void CreateFromSpanTest()
    {
        for (int length = 0; length <= 10_000; length *= 10)
        {
            ReadOnlySpan<char> span = Random.Shared.NextString(length);
            using UnmanagedString unmanagedString = UnmanagedString.Create(span);

            Assert.AreEqual(span.Length, unmanagedString.Length);
            Assert.AreEqual(span.Length, unmanagedString.String.Length);
            Assert.AreEqual(span.Length, unmanagedString.AsSpan().Length);

            Assert.IsTrue(unmanagedString.AsSpan().SequenceEqual(unmanagedString.String));
            Assert.IsTrue(span.SequenceEqual(unmanagedString.String));

            if (length == 0)
            {
                length = 1;
            }
        }
    }
}
