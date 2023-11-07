using System;
using System.Diagnostics.CodeAnalysis;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed class BufferHelpersTest
{
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
    public static object[][] GrowByPow2Parameters { get; } =
    [
        [8, 4, 1],
        [16, 4, 5],
        [32, 0, 32],
        [128, 8, 120],
        [256, 8, 121],
        [1 << 16, 1 << 15, 1],
        [int.MaxValue, 1 << 30, 1],
        [int.MaxValue, int.MaxValue, 0]
    ];

    [Theory]
    [MemberData(nameof(GrowByPow2Parameters))]
    public void GrowByPow2Test(int expected, int currentLength, int neededSize)
        => Assert.Equal(expected, BufferHelpers.GrowByPow2(currentLength, neededSize));

    [Fact]
    public void GrowByPow2_Throws_Test()
    {
        Assert.Throws<InvalidOperationException>(static () => BufferHelpers.GrowByPow2(int.MaxValue, 1));
        Assert.Throws<InvalidOperationException>(static () => BufferHelpers.GrowByPow2(int.MaxValue, int.MaxValue));
        Assert.Throws<InvalidOperationException>(static () => BufferHelpers.GrowByPow2(1 << 30, 1 << 30));
        Assert.Throws<ArgumentOutOfRangeException>(static () => BufferHelpers.GrowByPow2(16, -1));
        Assert.Throws<ArgumentOutOfRangeException>(static () => BufferHelpers.GrowByPow2(-1, 16));
    }
}
