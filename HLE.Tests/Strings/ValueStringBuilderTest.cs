using System;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class ValueStringBuilderTest
{
    [TestMethod]
    public void AppendCharTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        builder.Append('a');
        builder.Append('b', 'c');
        Assert.AreEqual("abc", builder.ToString());
    }

    [TestMethod]
    public void AppendReadOnlySpanOfCharTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        builder.Append("he");
        builder.Append("ll", "oo");
        Assert.AreEqual("helloo", builder.ToString());
    }

    [TestMethod]
    public void AppendByteTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        const byte value = 123;
        builder.Append(value);
        Assert.AreEqual("123", builder.ToString());

        builder.Clear();
        const byte hexValue = 0xAF;
        builder.Append(hexValue, "X");
        Assert.AreEqual("AF", builder.ToString());
    }

    [TestMethod]
    public void AppendSByteTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        const sbyte value = 123;
        builder.Append(value);
        Assert.AreEqual("123", builder.ToString());

        builder.Clear();
        const sbyte hexValue = 0x3F;
        builder.Append(hexValue, "X");
        Assert.AreEqual("3F", builder.ToString());
    }

    [TestMethod]
    public void AppendShortTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        const short value = 12353;
        builder.Append(value);
        Assert.AreEqual("12353", builder.ToString());

        builder.Clear();
        const short hexValue = 0x3FAC;
        builder.Append(hexValue, "X");
        Assert.AreEqual("3FAC", builder.ToString());
    }

    [TestMethod]
    public void AppendUShortTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        const ushort value = 60123;
        builder.Append(value);
        Assert.AreEqual("60123", builder.ToString());

        builder.Clear();
        const ushort hexValue = 0x3FAC;
        builder.Append(hexValue, "X");
        Assert.AreEqual("3FAC", builder.ToString());
    }

    [TestMethod]
    public void AppendIntTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        const int value = 82793498;
        builder.Append(value);
        Assert.AreEqual("82793498", builder.ToString());

        builder.Clear();
        const int hexValue = 0x3FACCBFF;
        builder.Append(hexValue, "X");
        Assert.AreEqual("3FACCBFF", builder.ToString());
    }

    [TestMethod]
    public void AppendUIntTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        const uint value = 827934982;
        builder.Append(value);
        Assert.AreEqual("827934982", builder.ToString());

        builder.Clear();
        const uint hexValue = 0x3FACCBFF;
        builder.Append(hexValue, "X");
        Assert.AreEqual("3FACCBFF", builder.ToString());
    }

    [TestMethod]
    public void AppendLongTest()
    {
        ValueStringBuilder builder = stackalloc char[20];
        builder.Append(long.MaxValue);
        Assert.AreEqual(long.MaxValue.ToString(), builder.ToString());

        builder.Clear();
        builder.Append(long.MaxValue, "X");
        Assert.AreEqual(long.MaxValue.ToString("X"), builder.ToString());
    }

    [TestMethod]
    public void AppendULongTest()
    {
        ValueStringBuilder builder = stackalloc char[20];
        builder.Append(ulong.MaxValue);
        Assert.AreEqual(ulong.MaxValue.ToString(), builder.ToString());

        builder.Clear();
        builder.Append(ulong.MaxValue, "X");
        Assert.AreEqual(ulong.MaxValue.ToString("X"), builder.ToString());
    }

    [TestMethod]
    public void AppendFloatTest()
    {
        ValueStringBuilder builder = stackalloc char[20];
        const float value = 1.5f;
        builder.Append(value);
        Assert.AreEqual("1.5", builder.ToString());
    }

    [TestMethod]
    public void AppendDoubleTest()
    {
        ValueStringBuilder builder = stackalloc char[20];
        const double value = 10.5;
        builder.Append(value);
        Assert.AreEqual("10.5", builder.ToString());
    }

    [TestMethod]
    public void AppendDateTimeTest()
    {
        ValueStringBuilder builder = stackalloc char[50];
        DateTime now = DateTime.UtcNow;
        builder.Append(now, "O");
        Assert.AreEqual(now.ToString("O"), builder.ToString());
    }

    [TestMethod]
    public void AppendDateTimeOffsetTest()
    {
        ValueStringBuilder builder = stackalloc char[50];
        DateTimeOffset now = DateTimeOffset.UtcNow;
        builder.Append(now, "O");
        Assert.AreEqual(now.ToString("O"), builder.ToString());
    }

    [TestMethod]
    public void AppendISpanFormattableTest()
    {
        ValueStringBuilder builder = stackalloc char[50];
        ISpanFormattable now = DateTimeOffset.UtcNow;
        builder.Append<ISpanFormattable, IFormatProvider>(now, "O");

        Span<char> formatResult = stackalloc char[50];
        now.TryFormat(formatResult, out int charsWritten, "O", null);

        Assert.AreEqual(new(formatResult[..charsWritten]), builder.ToString());
    }

    [TestMethod]
    public void ClearTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        builder.Append("aowjdho");
        builder.Clear();
        Assert.AreEqual(string.Empty, builder.ToString());
        Assert.IsTrue(builder.Length == 0);
    }

    [TestMethod]
    public void AdvanceTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        "hello".CopyTo(builder.FreeBuffer);
        builder.Advance("hello".Length);
        Assert.AreEqual("hello", builder.ToString());
    }

    [TestMethod]
    public void RemoveTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        builder.Append("helllllllo");
        builder.Remove(2, 5);
        Assert.AreEqual("hello", builder.ToString());
    }

    [TestMethod]
    public void ToCharArrayTest()
    {
        ValueStringBuilder builder = stackalloc char[10];
        builder.Append("hello");
        Span<char> chars = builder.ToCharArray();
        Assert.IsTrue(chars is "hello");
    }

    [TestMethod]
    public void Equals_BufferReference_Test()
    {
        ValueStringBuilder builder = stackalloc char[10];
        ValueStringBuilder builder2 = stackalloc char[10];
        Assert.IsTrue(builder.Equals(builder));
        Assert.IsFalse(builder.Equals(builder2));
        Assert.IsTrue(builder != builder2);
    }

    public void Equals_SpanSequence_Test()
    {
        ValueStringBuilder builder = stackalloc char[10];
        ValueStringBuilder builder2 = stackalloc char[10];
        builder.Append("hello");
        builder2.Append("HELLO");
        Assert.IsTrue(builder.Equals(builder2, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(builder.Equals(builder2, StringComparison.Ordinal));
    }
}
