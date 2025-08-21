using System.Diagnostics.CodeAnalysis;

namespace HLE.Memory.UnitTests;

[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
[SuppressMessage("ReSharper", "RedundantOverflowCheckingContext")]
public sealed class PackedHelpersTest
{
    [Fact]
    public void CreateInt16_FromUInt8()
    {
        byte lower = 0x12;
        byte upper = 0x34;
        short result = PackedHelpers.CreateInt16(lower, upper);
        Assert.Equal(0x3412, result);
    }

    [Fact]
    public void CreateInt16_FromInt8()
    {
        sbyte lower = 0x12;
        sbyte upper = 0x34;
        short result = PackedHelpers.CreateInt16(lower, upper);
        Assert.Equal(0x3412, result);
    }

    [Fact]
    public void CreateUInt16_FromUInt8()
    {
        byte lower = 0x12;
        byte upper = 0x34;
        ushort result = PackedHelpers.CreateUInt16(lower, upper);
        Assert.Equal(0x3412, result);
    }

    [Fact]
    public void CreateUInt16_FromInt8()
    {
        sbyte lower = 0x12;
        sbyte upper = 0x34;
        ushort result = PackedHelpers.CreateUInt16(lower, upper);
        Assert.Equal(0x3412, result);
    }

    [Fact]
    public void CreateInt32_FromUInt8()
    {
        byte b1 = 0x12;
        byte b2 = 0x34;
        byte b3 = 0x56;
        byte b4 = 0x78;
        int result = PackedHelpers.CreateInt32(b1, b2, b3, b4);
        Assert.Equal(0x78563412, result);
    }

    [Fact]
    public void CreateInt32_FromInt8()
    {
        sbyte b1 = 0x12;
        sbyte b2 = 0x34;
        sbyte b3 = 0x56;
        sbyte b4 = 0x78;
        int result = PackedHelpers.CreateInt32(b1, b2, b3, b4);
        Assert.Equal(0x78563412, result);
    }

    [Fact]
    public void CreateUInt32_FromUInt8()
    {
        byte b1 = 0x12;
        byte b2 = 0x34;
        byte b3 = 0x56;
        byte b4 = 0x78;
        uint result = PackedHelpers.CreateUInt32(b1, b2, b3, b4);
        Assert.Equal(0x78563412U, result);
    }

    [Fact]
    public void CreateUInt32_FromInt8()
    {
        sbyte b1 = 0x12;
        sbyte b2 = 0x34;
        sbyte b3 = 0x56;
        sbyte b4 = 0x78;
        uint result = PackedHelpers.CreateUInt32(b1, b2, b3, b4);
        Assert.Equal(0x78563412U, result);
    }

    [Fact]
    public void CreateInt32_FromUInt16()
    {
        ushort lower = 0x1234;
        ushort upper = 0x5678;
        int result = PackedHelpers.CreateInt32(lower, upper);
        Assert.Equal(0x56781234, result);
    }

    [Fact]
    public void CreateInt32_FromInt16()
    {
        short lower = 0x1234;
        short upper = 0x5678;
        int result = PackedHelpers.CreateInt32(lower, upper);
        Assert.Equal(0x56781234, result);
    }

    [Fact]
    public void CreateUInt32_FromUInt16()
    {
        ushort lower = 0x1234;
        ushort upper = 0x5678;
        uint result = PackedHelpers.CreateUInt32(lower, upper);
        Assert.Equal(0x56781234U, result);
    }

    [Fact]
    public void CreateUInt32_FromInt16()
    {
        short lower = 0x1234;
        short upper = 0x5678;
        uint result = PackedHelpers.CreateUInt32(lower, upper);
        Assert.Equal(0x56781234U, result);
    }

    [Fact]
    public void CreateInt64_FromUInt8()
    {
        byte b1 = 0x12;
        byte b2 = 0x34;
        byte b3 = 0x56;
        byte b4 = 0x78;
        byte b5 = 0x9A;
        byte b6 = 0xBC;
        byte b7 = 0xDE;
        byte b8 = 0xF0;
        long result = PackedHelpers.CreateInt64(b1, b2, b3, b4, b5, b6, b7, b8);
        Assert.Equal(unchecked((long)0xF0DEBC9A78563412L), result);
    }

    [Fact]
    public void CreateInt64_FromInt8()
    {
        sbyte b1 = 0x12;
        sbyte b2 = 0x34;
        sbyte b3 = 0x56;
        sbyte b4 = 0x78;
        sbyte b5 = unchecked((sbyte)0x9A);
        sbyte b6 = unchecked((sbyte)0xBC);
        sbyte b7 = unchecked((sbyte)0xDE);
        sbyte b8 = unchecked((sbyte)0xF0);
        long result = PackedHelpers.CreateInt64(b1, b2, b3, b4, b5, b6, b7, b8);
        Assert.Equal(unchecked((long)0xF0DEBC9A78563412), result);
    }

    [Fact]
    public void CreateUInt64_FromUInt8()
    {
        byte b1 = 0x12;
        byte b2 = 0x34;
        byte b3 = 0x56;
        byte b4 = 0x78;
        byte b5 = 0x9A;
        byte b6 = 0xBC;
        byte b7 = 0xDE;
        byte b8 = 0xF0;
        ulong result = PackedHelpers.CreateUInt64(b1, b2, b3, b4, b5, b6, b7, b8);
        Assert.Equal(0xF0DEBC9A78563412UL, result);
    }

    [Fact]
    public void CreateUInt64_FromInt8()
    {
        sbyte b1 = 0x12;
        sbyte b2 = 0x34;
        sbyte b3 = 0x56;
        sbyte b4 = 0x78;
        sbyte b5 = unchecked((sbyte)0x9A);
        sbyte b6 = unchecked((sbyte)0xBC);
        sbyte b7 = unchecked((sbyte)0xDE);
        sbyte b8 = unchecked((sbyte)0xF0);
        ulong result = PackedHelpers.CreateUInt64(b1, b2, b3, b4, b5, b6, b7, b8);
        Assert.Equal(0xF0DEBC9A78563412UL, result);
    }

    [Fact]
    public void CreateInt64_FromUInt16()
    {
        ushort lower1 = 0x1234;
        ushort lower2 = 0x5678;
        ushort upper1 = 0x9ABC;
        ushort upper2 = 0xDEF0;
        long result = PackedHelpers.CreateInt64(lower1, lower2, upper1, upper2);
        Assert.Equal(unchecked((long)0xDEF09ABC56781234), result);
    }

    [Fact]
    public void CreateInt64_FromInt16()
    {
        short lower1 = 0x1234;
        short lower2 = 0x5678;
        short upper1 = unchecked((short)0x9ABC);
        short upper2 = unchecked((short)0xDEF0);
        long result = PackedHelpers.CreateInt64(lower1, lower2, upper1, upper2);
        Assert.Equal(unchecked((long)0xDEF09ABC56781234), result);
    }

    [Fact]
    public void CreateUInt64_FromUInt16()
    {
        ushort lower1 = 0x1234;
        ushort lower2 = 0x5678;
        ushort upper1 = 0x9ABC;
        ushort upper2 = 0xDEF0;
        ulong result = PackedHelpers.CreateUInt64(lower1, lower2, upper1, upper2);
        Assert.Equal(0xDEF09ABC56781234UL, result);
    }

    [Fact]
    public void CreateUInt64_FromInt16()
    {
        short lower1 = 0x1234;
        short lower2 = 0x5678;
        short upper1 = unchecked((short)0x9ABC);
        short upper2 = unchecked((short)0xDEF0);
        ulong result = PackedHelpers.CreateUInt64(lower1, lower2, upper1, upper2);
        Assert.Equal(0xDEF09ABC56781234UL, result);
    }

    [Fact]
    public void CreateInt64_FromUInt32()
    {
        uint lower = 0x12345678;
        uint upper = 0x9ABCDEF0;
        long result = PackedHelpers.CreateInt64(lower, upper);
        Assert.Equal(unchecked((long)0x9ABCDEF012345678), result);
    }

    [Fact]
    public void CreateInt64_FromInt32()
    {
        int lower = 0x12345678;
        int upper = unchecked((int)0x9ABCDEF0);
        long result = PackedHelpers.CreateInt64(lower, upper);
        Assert.Equal(unchecked((long)0x9ABCDEF012345678), result);
    }

    [Fact]
    public void CreateUInt64_FromUInt32()
    {
        uint lower = 0x12345678;
        uint upper = 0x9ABCDEF0;
        ulong result = PackedHelpers.CreateUInt64(lower, upper);
        Assert.Equal(0x9ABCDEF012345678UL, result);
    }

    [Fact]
    public void CreateUInt64_FromInt32()
    {
        int lower = 0x12345678;
        int upper = unchecked((int)0x9ABCDEF0);
        ulong result = PackedHelpers.CreateUInt64(lower, upper);
        Assert.Equal(0x9ABCDEF012345678UL, result);
    }
}
