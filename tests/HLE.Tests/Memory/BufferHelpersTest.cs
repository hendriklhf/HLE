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
        [Array.MaxLength, 1 << 30, 1],
        [Array.MaxLength, Array.MaxLength, 0]
    ];

    [Fact]
    [SuppressMessage("Assertions", "xUnit2000:Constants and literals should be the expected argument", Justification = "Array.MaxLength is correctly expected.")]
    public void MaximumArrayLengthIsInSyncWithArrayMaxLengthTest()
        => Assert.Equal(Array.MaxLength, BufferHelpers.MaximumArrayLength);

    [Theory]
    [MemberData(nameof(GrowByPow2Parameters))]
    public void GrowByPow2Test(int expected, int currentLength, int neededSize)
        => Assert.Equal(expected, BufferHelpers.GrowArray(currentLength, neededSize));

    [Fact]
    public void GrowByPow2_Throws_Test()
    {
        Assert.Throws<InvalidOperationException>(static () => BufferHelpers.GrowArray(int.MaxValue, 1));
        Assert.Throws<InvalidOperationException>(static () => BufferHelpers.GrowArray(int.MaxValue, int.MaxValue));
        Assert.Throws<InvalidOperationException>(static () => BufferHelpers.GrowArray(1 << 30, 1 << 30));
        Assert.Throws<ArgumentOutOfRangeException>(static () => BufferHelpers.GrowArray(16, -1));
        Assert.Throws<ArgumentOutOfRangeException>(static () => BufferHelpers.GrowArray(-1, 16));
    }
}
