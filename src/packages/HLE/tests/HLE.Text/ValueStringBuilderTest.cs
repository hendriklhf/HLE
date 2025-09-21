using System;
using System.Globalization;
using HLE;
using HLE.Text;

namespace HLE.UnitTests.HLE.Text;

public sealed class ValueStringBuilderTest
{
    [Fact]
    public void Indexer_Int32_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal('h', builder[0]);

        ref char firstChar = ref builder[0];
        firstChar = 'x';
        Assert.Equal("xello", builder.ToString());
    }

    [Fact]
    public void Indexer_Index_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal('o', builder[^1]);

        ref char lastChar = ref builder[^1];
        lastChar = 'x';
        Assert.Equal("hellx", builder.ToString());
    }

    [Fact]
    public void Indexer_Range_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Span<char> range = builder[..2];
        range.Fill('l');

        Assert.Equal("llllo", builder.ToString());
    }

    [Fact]
    public void LengthTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");
        Assert.Equal("hello".Length, builder.Length);

        builder.Append("123");
        Assert.Equal("hello123".Length, builder.Length);
        Assert.Equal("hello123", builder.ToString());
    }

    [Fact]
    public void CapacityTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);
        Assert.Equal(1000, builder.Capacity);
    }

    [Fact]
    public void BufferSpanTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal(10, builder.GetBuffer().Length);
        Assert.True(builder.GetBuffer().StartsWith("hello"));
    }

    [Fact]
    public void WrittenSpanTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.Equal("hello", new(builder.WrittenSpan));
    }

    [Fact]
    public void FreeBufferSpanTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.True(builder.FreeBufferSpan.Length == builder.Capacity - builder.Length);
    }

    [Fact]
    public void FreeBufferSizeTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.True(builder.FreeBufferSize == builder.Capacity - builder.Length);
    }

    [Fact]
    public void Constructor_NoParameter_Test()
    {
        using ValueStringBuilder builder = new();
        Assert.Equal(0, builder.Capacity);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Constructor_InitialBufferSize_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        Assert.Equal(10, builder.Capacity);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void AdvanceTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        "hello".CopyTo(builder.FreeBufferSpan);
        builder.Advance("hello".Length);
        "hello".CopyTo(builder.FreeBufferSpan);
        builder.Advance("hello".Length);

        Assert.Equal("hellohello", builder.ToString());

        builder.Advance(-"hello".Length);
        Assert.Equal("hello", builder.ToString());
    }

    [Fact]
    public void Append_ReadOnlySpan_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1005]);
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
        using ValueStringBuilder builder = new(stackalloc char[20]);
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
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const byte Value = 255;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_SByte_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const sbyte Value = 120;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_Short_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const short Value = 30_000;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_UShort_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const ushort Value = 60_000;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_Int_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const int Value = int.MaxValue;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_UInt_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const uint Value = uint.MaxValue;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_Long_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const long Value = long.MaxValue;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_ULong_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const ulong Value = ulong.MaxValue;
        builder.Append(Value);

        Assert.Equal(Value.ToString(), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString().Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_Float_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const float Value = float.Pi;
        builder.Append(Value);

        Assert.Equal(Value.ToString(CultureInfo.InvariantCulture), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString(CultureInfo.InvariantCulture).Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_Double_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

        const double Value = double.Pi;
        builder.Append(Value);

        Assert.Equal(Value.ToString(CultureInfo.InvariantCulture), builder.ToString());

        for (int i = 0; i < 5; i++)
        {
            builder.Append(Value);
        }

        Assert.Equal(Value.ToString(CultureInfo.InvariantCulture).Length * 6, builder.Length);
        Assert.Equal($"{Value}{Value}{Value}{Value}{Value}{Value}", builder.ToString());
    }

    [Fact]
    public void Append_DateTime_Test()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);

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
        using ValueStringBuilder builder = new(stackalloc char[1000]);

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
        using ValueStringBuilder builder = new(stackalloc char[1000]);

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
        using ValueStringBuilder builder = new(stackalloc char[1000]);
        builder.Append(Random.Shared.NextString(1000));
        Assert.Equal(1000, builder.Length);

        builder.Clear();
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void ToStringTest()
    {
        using ValueStringBuilder builder = new(stackalloc char[1000]);
        string randomString = Random.Shared.NextString(1000);
        builder.Append(randomString);

        Assert.Equal(randomString, builder.ToString());
    }

    [Fact]
    public void Equals_ValueStringBuilder_StringComparison_Test()
    {
        using ValueStringBuilder builder1 = new(stackalloc char[10]);
        using ValueStringBuilder builder2 = new(stackalloc char[10]);

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.True(builder1.Equals(builder2, StringComparison.OrdinalIgnoreCase));
        Assert.False(builder1.Equals(builder2, StringComparison.Ordinal));
    }

    [Fact]
    public void Equals_ReadOnlySpanChar_StringComparison()
    {
        using ValueStringBuilder builder = new(stackalloc char[10]);
        builder.Append("hello");

        Assert.True(builder.Equals("HELLO", StringComparison.OrdinalIgnoreCase));
        Assert.False(builder.Equals("HELLO", StringComparison.Ordinal));
    }

    [Fact]
    public void Equals_ValueStringBuilder_Test()
    {
        using ValueStringBuilder builder1 = new(stackalloc char[10]);
        using ValueStringBuilder builder2 = new(stackalloc char[10]);

        builder1.Append("HELLO");
        builder2.Append("hello");

        Assert.True(builder1.Equals(builder1, StringComparison.Ordinal));
        Assert.False(builder1.Equals(builder2, StringComparison.Ordinal));

        Assert.False(builder1 == builder2);
    }
}
