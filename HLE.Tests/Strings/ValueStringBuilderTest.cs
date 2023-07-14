using System;
using System.Globalization;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class ValueStringBuilderTest
{
    [TestMethod]
    public void Indexer_Int32_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.AreEqual('h', builder[0]);

        ref char firstChar = ref builder[0];
        firstChar = 'x';
        Assert.AreEqual("xello", builder.ToString());
    }

    [TestMethod]
    public void Indexer_Index_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.AreEqual('o', builder[^1]);

        ref char lastChar = ref builder[^1];
        lastChar = 'x';
        Assert.AreEqual("hellx", builder.ToString());
    }

    [TestMethod]
    public void Indexer_Range_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Span<char> range = builder[..2];
        range.Fill('l');

        Assert.AreEqual("llllo", builder.ToString());
    }

    [TestMethod]
    public void LengthTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");
        Assert.AreEqual("hello".Length, builder.Length);

        builder.Append("123");
        Assert.AreEqual("hello123".Length, builder.Length);
        Assert.AreEqual("hello123", builder.ToString());
    }

    [TestMethod]
    public void CapacityTest()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);
        Assert.AreEqual(1000, builder.Capacity);
    }

    [TestMethod]
    public void BufferSpanTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.AreEqual(10, builder._buffer.Length);
        Assert.IsTrue(builder._buffer.StartsWith("hello"));
    }

    [TestMethod]
    public void WrittenSpanTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.AreEqual("hello", new(builder.WrittenSpan));
    }

    [TestMethod]
    public void FreeBufferSpanTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.IsTrue(builder.FreeBuffer.Length == builder.Capacity - builder.Length);
    }

    [TestMethod]
    public void FreeBufferSizeTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.IsTrue(builder.FreeBufferSize == builder.Capacity - builder.Length);
    }

    [TestMethod]
    public void EmptyTest()
    {
        ValueStringBuilder builder = ValueStringBuilder.Empty;
        Assert.AreEqual(0, builder.Length);
        Assert.AreEqual(0, builder.Capacity);
    }

    [TestMethod]
    public void Constructor_NoParameter_Test()
    {
        ValueStringBuilder builder = new();
        Assert.AreEqual(0, builder.Capacity);
        Assert.AreEqual(0, builder.Length);
    }

    [TestMethod]
    public void Constructor_InitialBufferSize_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        Assert.AreEqual(10, builder.Capacity);
        Assert.AreEqual(0, builder.Length);
    }

    [TestMethod]
    public void AdvanceTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        "hello".CopyTo(builder.FreeBuffer);
        builder.Advance("hello".Length);
        "hello".CopyTo(builder.FreeBuffer);
        builder.Advance("hello".Length);

        Assert.AreEqual("hellohello", builder.ToString());

        builder.Advance(-"hello".Length);
        Assert.AreEqual("hello", builder.ToString());
    }

    [TestMethod]
    public void Append_ReadOnlySpan_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1005]);
        Assert.AreEqual(1005, builder.Capacity);

        builder.Append("hello");
        Assert.AreEqual("hello", builder.ToString());

        string randomString = Random.Shared.NextString(1000);
        builder.Append(randomString);

        Assert.AreEqual(1005, builder.Length);
        Assert.AreEqual(0, builder.FreeBufferSize);
        Assert.AreEqual("hello" + randomString, builder.ToString());
    }

    [TestMethod]
    public void Append_Char_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[20]);
        Assert.AreEqual(20, builder.Capacity);

        for (int i = 0; i < 20; i++)
        {
            builder.Append('a');
        }

        Assert.AreEqual(new('a', 20), builder.ToString());
        Assert.AreEqual(20, builder.Length);
    }

    [TestMethod]
    public void Append_Byte_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const byte value = 255;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_SByte_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const sbyte value = 120;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_Short_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const short value = 30_000;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_UShort_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const ushort value = 60_000;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_Int_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const int value = int.MaxValue;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_UInt_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const uint value = uint.MaxValue;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_Long_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const long value = long.MaxValue;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_ULong_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const ulong value = ulong.MaxValue;
        builder.Append(value);

        Assert.AreEqual(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString().Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_Float_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const float value = float.Pi;
        builder.Append(value);

        Assert.AreEqual(value.ToString(CultureInfo.InvariantCulture), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString(CultureInfo.InvariantCulture).Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_Double_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const double value = double.Pi;
        builder.Append(value);

        Assert.AreEqual(value.ToString(CultureInfo.InvariantCulture), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.AreEqual(value.ToString(CultureInfo.InvariantCulture).Length * 6, builder.Length);
        Assert.AreEqual($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [TestMethod]
    public void Append_DateTime_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        DateTime value = DateTime.UtcNow;
        builder.Append(value, "O");

        Assert.AreEqual(value.ToString("O"), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value, "O");
        }

        Assert.AreEqual(value.ToString("O").Length * 6, builder.Length);
        Assert.AreEqual($"{value:O}{value:O}{value:O}{value:O}{value:O}{value:O}", builder.ToString());
    }

    [TestMethod]
    public void Append_DateTimeOffset_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        DateTimeOffset value = DateTimeOffset.UtcNow;
        builder.Append(value, "O");

        Assert.AreEqual(value.ToString("O"), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value, "O");
        }

        Assert.AreEqual(value.ToString("O").Length * 6, builder.Length);
        Assert.AreEqual($"{value:O}{value:O}{value:O}{value:O}{value:O}{value:O}", builder.ToString());
    }

    [TestMethod]
    public void Append_TimeSpan_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        TimeSpan value = TimeSpan.FromHours(10);
        builder.Append(value, "G");

        Assert.AreEqual(value.ToString("G"), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value, "G");
        }

        Assert.AreEqual(value.ToString("G").Length * 6, builder.Length);
        Assert.AreEqual($"{value:G}{value:G}{value:G}{value:G}{value:G}{value:G}", builder.ToString());
    }

    [TestMethod]
    public void ClearTest()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);
        builder.Append(Random.Shared.NextString(1000));
        Assert.AreEqual(1000, builder.Length);

        builder.Clear();
        Assert.AreEqual(0, builder.Length);
    }

    [TestMethod]
    public void ToStringTest()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);
        string randomString = Random.Shared.NextString(1000);
        builder.Append(randomString);

        Assert.AreEqual(randomString, builder.ToString());
    }

    [TestMethod]
    public void Equals_ValueStringBuilder_StringComparison_Test()
    {
        ValueStringBuilder builder1 = new(stackalloc char[10]);
        ValueStringBuilder builder2 = new(stackalloc char[10]);

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.IsTrue(builder1.Equals(builder2, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(builder1.Equals(builder2, StringComparison.Ordinal));
    }

    [TestMethod]
    public void Equals_ReadOnlySpanChar_StringComparison()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.IsTrue(builder.Equals("HELLO", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(builder.Equals("HELLO", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Equals_ValueStringBuilder_Test()
    {
        ValueStringBuilder builder1 = new(stackalloc char[10]);
        ValueStringBuilder builder2 = new(stackalloc char[10]);

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.IsTrue(builder1.Equals(builder1));
        Assert.IsFalse(builder1.Equals(builder2));

        Assert.IsFalse(builder1 == builder2);
    }
}
