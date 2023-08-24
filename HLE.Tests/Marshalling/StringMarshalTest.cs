using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Marshalling;

[TestClass]
public class StringMarshalTest
{
    [TestMethod]
    public void FastAllocateStringTest()
    {
        string str = StringMarshal.FastAllocateString(5, out Span<char> chars);
        "hello".CopyTo(chars);
        Assert.AreEqual("hello", str);
        Assert.AreEqual("hello".Length, str.Length);
        Assert.IsFalse(ReferenceEquals("hello", str));
    }

    [TestMethod]
    public void AsMutableSpanTest()
    {
        const string str = "hello";
        Span<char> chars = StringMarshal.AsMutableSpan(str);
        Assert.IsTrue(chars is "hello");
    }

    [TestMethod]
    public void AsStringTest()
    {
        ReadOnlySpan<char> span = "hello";
        string str = StringMarshal.AsString(span);
        Assert.IsTrue(span.SequenceEqual(str));

        ref char spanRef = ref MemoryMarshal.GetReference(span);
        ref char strRef = ref MemoryMarshal.GetReference(str.AsSpan());
        Assert.IsTrue(Unsafe.AreSame(ref spanRef, ref strRef));
    }
}
