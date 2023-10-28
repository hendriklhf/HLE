using System;
using System.Globalization;
using HLE.Strings;
using Xunit;

namespace HLE.Tests.Strings;

public sealed class ValueStringBuilderTest
{
    [Fact]
    public void Indexer_Int32_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal('h', builder[0]);

        ref char firstChar = ref builder[0];
        firstChar = 'x';
        Assert.Equal("xello", builder.ToString());
    }

    [Fact]
    public void Indexer_Index_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal('o', builder[^1]);

        ref char lastChar = ref builder[^1];
        lastChar = 'x';
        Assert.Equal("hellx", builder.ToString());
    }

    [Fact]
    public void Indexer_Range_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Span<char> range = builder[..2];
        range.Fill('l');

        Assert.Equal("llllo", builder.ToString());
    }

    [Fact]
    public void LengthTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");
        Assert.Equal("hello".Length, builder.Length);

        builder.Append("123");
        Assert.Equal("hello123".Length, builder.Length);
        Assert.Equal("hello123", builder.ToString());
    }

    [Fact]
    public void CapacityTest()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);
        Assert.Equal(1000, builder.Capacity);
    }

    [Fact]
    public void BufferSpanTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal(10, builder._buffer.Length);
        Assert.True(builder._buffer.StartsWith("hello"));
    }

    [Fact]
    public void WrittenSpanTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal("hello", new(builder.WrittenSpan));
    }

    [Fact]
    public void FreeBufferSpanTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.True(builder.FreeBuffer.Length == builder.Capacity - builder.Length);
    }

    [Fact]
    public void FreeBufferSizeTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.True(builder.FreeBufferSize == builder.Capacity - builder.Length);
    }

    [Fact]
    public void EmptyTest()
    {
        ValueStringBuilder builder = ValueStringBuilder.Empty;
        Assert.Equal(0, builder.Length);
        Assert.Equal(0, builder.Capacity);
    }

    [Fact]
    public void Constructor_NoParameter_Test()
    {
        ValueStringBuilder builder = new();
        Assert.Equal(0, builder.Capacity);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Constructor_InitialBufferSize_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        Assert.Equal(10, builder.Capacity);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void AdvanceTest()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        "hello".CopyTo(builder.FreeBuffer);
        builder.Advance("hello".Length);
        "hello".CopyTo(builder.FreeBuffer);
        builder.Advance("hello".Length);

        Assert.Equal("hellohello", builder.ToString());

        builder.Advance(-"hello".Length);
        Assert.Equal("hello", builder.ToString());
    }

    [Fact]
    public void Append_ReadOnlySpan_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1005]);
        Assert.Equal(1005, builder.Capacity);

        builder.Append("hello");
        Assert.Equal("hello", builder.ToString());

        string randomString = Random.Shared.NextString(1000);
        builder.Append(randomString);

        Assert.Equal(1005, builder.Length);
        Assert.Equal(0, builder.FreeBufferSize);
        Assert.Equal("hello" + randomString, builder.ToString());
    }

    [Fact]
    public void Append_Char_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[20]);
        Assert.Equal(20, builder.Capacity);

        for (int i = 0; i < 20; i++)
        {
            builder.Append('a');
        }

        Assert.Equal(new('a', 20), builder.ToString());
        Assert.Equal(20, builder.Length);
    }

    [Fact]
    public void Append_Byte_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const byte value = 255;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_SByte_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const sbyte value = 120;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_Short_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const short value = 30_000;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_UShort_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const ushort value = 60_000;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_Int_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const int value = int.MaxValue;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_UInt_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const uint value = uint.MaxValue;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_Long_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const long value = long.MaxValue;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_ULong_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const ulong value = ulong.MaxValue;
        builder.Append(value);

        Assert.Equal(value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_Float_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const float value = float.Pi;
        builder.Append(value);

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture).Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_Double_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        const double value = double.Pi;
        builder.Append(value);

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value);
        }

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture).Length * 6, builder.Length);
        Assert.Equal($"{value}{value}{value}{value}{value}{value}", builder.ToString());
    }

    [Fact]
    public void Append_DateTime_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        DateTime value = DateTime.UtcNow;
        builder.Append(value, "O");

        Assert.Equal(value.ToString("O"), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value, "O");
        }

        Assert.Equal(value.ToString("O").Length * 6, builder.Length);
        Assert.Equal($"{value:O}{value:O}{value:O}{value:O}{value:O}{value:O}", builder.ToString());
    }

    [Fact]
    public void Append_DateTimeOffset_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        DateTimeOffset value = DateTimeOffset.UtcNow;
        builder.Append(value, "O");

        Assert.Equal(value.ToString("O"), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value, "O");
        }

        Assert.Equal(value.ToString("O").Length * 6, builder.Length);
        Assert.Equal($"{value:O}{value:O}{value:O}{value:O}{value:O}{value:O}", builder.ToString());
    }

    [Fact]
    public void Append_TimeSpan_Test()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);

        TimeSpan value = TimeSpan.FromHours(10);
        builder.Append(value, "G");

        Assert.Equal(value.ToString("G"), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(value, "G");
        }

        Assert.Equal(value.ToString("G").Length * 6, builder.Length);
        Assert.Equal($"{value:G}{value:G}{value:G}{value:G}{value:G}{value:G}", builder.ToString());
    }

    [Fact]
    public void ClearTest()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);
        builder.Append(Random.Shared.NextString(1000));
        Assert.Equal(1000, builder.Length);

        builder.Clear();
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void ToStringTest()
    {
        ValueStringBuilder builder = new(stackalloc char[1000]);
        string randomString = Random.Shared.NextString(1000);
        builder.Append(randomString);

        Assert.Equal(randomString, builder.ToString());
    }

    [Fact]
    public void Equals_ValueStringBuilder_StringComparison_Test()
    {
        ValueStringBuilder builder1 = new(stackalloc char[10]);
        ValueStringBuilder builder2 = new(stackalloc char[10]);

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.True(builder1.Equals(builder2, StringComparison.OrdinalIgnoreCase));
        Assert.False(builder1.Equals(builder2, StringComparison.Ordinal));
    }

    [Fact]
    public void Equals_ReadOnlySpanChar_StringComparison()
    {
        ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.True(builder.Equals("HELLO", StringComparison.OrdinalIgnoreCase));
        Assert.False(builder.Equals("HELLO", StringComparison.Ordinal));
    }

    [Fact]
    public void Equals_ValueStringBuilder_Test()
    {
        ValueStringBuilder builder1 = new(stackalloc char[10]);
        ValueStringBuilder builder2 = new(stackalloc char[10]);

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.True(builder1.Equals(builder1, StringComparison.Ordinal));
        Assert.False(builder1.Equals(builder2, StringComparison.Ordinal));

        Assert.False(builder1 == builder2);
    }
}
