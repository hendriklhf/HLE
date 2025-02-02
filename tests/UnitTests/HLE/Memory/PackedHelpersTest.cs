using HLE.Memory;
using Xunit;

namespace HLE.UnitTests.Memory;

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
}
