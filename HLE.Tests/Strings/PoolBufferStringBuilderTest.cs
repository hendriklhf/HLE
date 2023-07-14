using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class PoolBufferStringBuilderTest
{
    [TestMethod]
    public void Indexer_Int32_Test()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.AreEqual('h', builder[0]);

        ref char firstChar = ref builder[0];
        firstChar = 'x';
        Assert.AreEqual("xello", builder.ToString());
    }

    [TestMethod]
    public void Indexer_Index_Test()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.AreEqual('o', builder[^1]);

        ref char lastChar = ref builder[^1];
        lastChar = 'x';
        Assert.AreEqual("hellx", builder.ToString());
    }

    [TestMethod]
    public void Indexer_Range_Test()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Span<char> range = builder[..2];
        range.Fill('l');

        Assert.AreEqual("llllo", builder.ToString());
    }

    [TestMethod]
    public void LengthTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");
        Assert.AreEqual("hello".Length, builder.Length);

        builder.Append("123");
        Assert.AreEqual("hello123".Length, builder.Length);
        Assert.AreEqual("hello123", builder.ToString());
    }

    [TestMethod]
    public void CapacityTest()
    {
        using PooledStringBuilder builder = new();
        Assert.AreEqual(PooledStringBuilder.DefaultBufferSize, builder.Capacity);

        builder.Append(Random.Shared.NextString(1000));
        Assert.AreEqual(1000, builder.Length);
        Assert.IsTrue(builder.Capacity >= builder.Length);
    }

    [TestMethod]
    public void BufferSpanTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.AreEqual(PooledStringBuilder.DefaultBufferSize, builder._buffer.Span.Length);
        Assert.IsTrue(builder._buffer.Span.StartsWith("hello"));
    }

    [TestMethod]
    public void BufferMemoryTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.AreEqual(PooledStringBuilder.DefaultBufferSize, builder._buffer.Memory.Length);
        Assert.IsTrue(builder._buffer.Span.StartsWith("hello"));
    }

    [TestMethod]
    public void WrittenSpanTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.AreEqual("hello", new(builder.WrittenSpan));
    }

    [TestMethod]
    public void WrittenMemoryTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.AreEqual("hello", new(builder.WrittenMemory.Span));
    }

    [TestMethod]
    public void FreeBufferSpanTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.IsTrue(builder.FreeBufferSpan.Length == builder.Capacity - builder.Length);
    }

    [TestMethod]
    public void FreeBufferMemoryTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.IsTrue(builder.FreeBufferMemory.Length == builder.Capacity - builder.Length);
    }

    [TestMethod]
    public void FreeBufferSizeTest()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.IsTrue(builder.FreeBufferSize == builder.Capacity - builder.Length);
    }

    [TestMethod]
    public void EmptyTest()
    {
        PooledStringBuilder empty = PooledStringBuilder.Empty;
        Assert.AreEqual(0, empty.Length);
        Assert.AreEqual(0, empty.Capacity);
    }

    [TestMethod]
    public void Constructor_NoParameter_Test()
    {
        using PooledStringBuilder builder = new();
        Assert.AreEqual(PooledStringBuilder.DefaultBufferSize, builder.Capacity);
        Assert.AreEqual(0, builder.Length);
    }

    [TestMethod]
    public void Constructor_InitialBufferSize_Test()
    {
        using PooledStringBuilder builder = new(100);
        Assert.IsTrue(builder.Capacity >= 100);
        Assert.AreEqual(0, builder.Length);
    }

    [TestMethod]
    public void AdvanceTest()
    {
        using PooledStringBuilder builder = new();
        "hello".CopyTo(builder.FreeBufferSpan);
        builder.Advance("hello".Length);
        "hello".CopyTo(builder.FreeBufferSpan);
        builder.Advance("hello".Length);

        Assert.AreEqual("hellohello", builder.ToString());

        builder.Advance(-"hello".Length);
        Assert.AreEqual("hello", builder.ToString());
    }

    [TestMethod]
    public void Append_ReadOnlySpan_Test()
    {
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

        builder.Append("hello");
        Assert.AreEqual("hello", builder.ToString());

        string randomString = Random.Shared.NextString(1000);
        builder.Append(randomString);

        Assert.AreEqual(1005, builder.Length);
        Assert.IsTrue(builder.Capacity >= 1005);
        Assert.AreEqual("hello" + randomString, builder.ToString());
    }

    [TestMethod]
    public void Append_Char_Test()
    {
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

        for (int i = 0; i < 20; i++)
        {
            builder.Append('a');
        }

        Assert.AreEqual(new('a', 20), builder.ToString());
        Assert.AreEqual(20, builder.Length);
        Assert.IsTrue(builder.Capacity >= 20);
    }

    [TestMethod]
    public void Append_Byte_Test()
    {
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new(16);
        Assert.AreEqual(16, builder.Capacity);

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
        using PooledStringBuilder builder = new();
        builder.Append(Random.Shared.NextString(1000));
        Assert.AreEqual(1000, builder.Length);

        builder.Clear();
        Assert.AreEqual(0, builder.Length);
    }

    [TestMethod]
    public void ToStringTest()
    {
        using PooledStringBuilder builder = new();
        string randomString = Random.Shared.NextString(1000);
        builder.Append(randomString);

        Assert.AreEqual(randomString, builder.ToString());
    }

    [TestMethod]
    public unsafe void CopyTo_CharPointer_Test()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Span<char> destination = stackalloc char[10];
        builder.CopyTo((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination)));

        string destinationString = new(destination[..builder.Length]);
        Assert.AreEqual("hello", destinationString);
        Assert.AreEqual("hello", builder.ToString());
        Assert.AreEqual(destinationString, builder.ToString());
    }

    [TestMethod]
    public void Equals_PoolBufferStringBuilder_StringComparison_Test()
    {
        using PooledStringBuilder builder1 = new();
        using PooledStringBuilder builder2 = new();

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.IsTrue(builder1.Equals(builder2, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(builder1.Equals(builder2, StringComparison.Ordinal));
    }

    [TestMethod]
    public void Equals_ReadOnlySpanChar_StringComparison()
    {
        using PooledStringBuilder builder = new();
        builder.Append("hello");

        Assert.IsTrue(builder.Equals("HELLO", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(builder.Equals("HELLO", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Equals_PoolBufferStringBuilder_Test()
    {
        using PooledStringBuilder builder1 = new();
        using PooledStringBuilder builder2 = new();

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.IsTrue(builder1.Equals(builder1));
        Assert.IsFalse(builder1.Equals(builder2));

        Assert.IsFalse(builder1 == builder2);
    }
}
