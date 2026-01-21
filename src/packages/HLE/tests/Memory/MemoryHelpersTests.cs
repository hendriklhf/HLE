using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Numerics;
using HLE.TestUtilities;

namespace HLE.UnitTests.Memory;

public sealed class MemoryHelpersTests
{
    public static TheoryData<nuint> AlignmentParameters { get; } =
    [
        1,
        2,
        4,
        8,
        16,
        32,
        64,
        128,
        256,
        512,
        1024,
        2048,
        4096
    ];

    public static TheoryData<(nuint, nuint)> AlignAddressParameters { get; } =
        TheoryDataHelpers.CreateMatrix(
            AlignmentParameters,
            TheoryDataHelpers.CreateRange<nuint>(800, 1050)
        );

    [Theory]
    [MemberData(nameof(AlignmentParameters))]
    public unsafe void IsAligned_ReturnsTrue_ForAlignedPointer(nuint alignment)
    {
        for (nuint address = 0; address < ushort.MaxValue; address += alignment)
        {
            bool result = MemoryHelpers.IsAligned((void*)address, alignment);
            Assert.True(result);
        }
    }

    [Theory]
    [MemberData(nameof(AlignmentParameters))]
    public unsafe void IsAligned_ReturnsTrue_ForAlignedReference(nuint alignment)
    {
        for (nuint address = 0; address < ushort.MaxValue; address += alignment)
        {
            bool result = MemoryHelpers.IsAligned(ref Unsafe.AsRef<int>((void*)address), alignment);
            Assert.True(result);
        }
    }

    [Theory]
    [MemberData(nameof(AlignAddressParameters))]
    public unsafe void Align_Add_ReturnsAlignedAddress((nuint Alignment, nuint Address) parameters)
    {
        nuint alignedAddress = (nuint)MemoryHelpers.Align((int*)parameters.Address, parameters.Alignment, AlignmentMethod.Add);
        Assert.True(MemoryHelpers.IsAligned((void*)alignedAddress, parameters.Alignment));
        Assert.Equal<nuint>(0, alignedAddress % parameters.Alignment);
        Assert.True(alignedAddress >= parameters.Address);

        if (parameters.Address % parameters.Alignment == 0)
        {
            Assert.Equal(parameters.Address, alignedAddress);
        }
    }

    [Theory]
    [MemberData(nameof(AlignAddressParameters))]
    public unsafe void Align_Subtract_ReturnsAlignedAddress((nuint Alignment, nuint Address) parameters)
    {
        nuint alignedAddress = (nuint)MemoryHelpers.Align((int*)parameters.Address, parameters.Alignment, AlignmentMethod.Subtract);
        Assert.True(MemoryHelpers.IsAligned((void*)alignedAddress, parameters.Alignment));
        Assert.Equal<nuint>(0, alignedAddress % parameters.Alignment);
        Assert.True(alignedAddress <= parameters.Address);

        if (parameters.Address % parameters.Alignment == 0)
        {
            Assert.Equal(parameters.Address, alignedAddress);
        }
    }
}
