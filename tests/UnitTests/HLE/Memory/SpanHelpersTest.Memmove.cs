using System;
using System.Runtime.InteropServices;
using HLE.Memory;
using HLE.TestUtilities;
using Xunit;

namespace HLE.UnitTests.Memory;

public sealed partial class SpanHelpersTest
{
    public static TheoryData<int> MemmoveParameters { get; } = CreateMemmoveParameters();

    public static TheoryData<int> MemmoveUnalignedAndOverlappingParameters { get; } = TheoryDataHelpers.CreateRange(64, 2048);

    [Theory]
    [MemberData(nameof(MemmoveParameters))]
    public void Memmove_Bytes_Test(int byteCount)
    {
        using RentedArray<byte> sourceBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);
        using RentedArray<byte> destinationBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);

        Span<byte> source = sourceBuffer[..byteCount];
        Random.Shared.Fill(source);
        Span<byte> destination = destinationBuffer[..byteCount];

        SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);
        Assert.True(destination.SequenceEqual(source));
    }

    [Theory]
    [MemberData(nameof(MemmoveUnalignedAndOverlappingParameters))]
    public void Memmove_Unaligned_Test(int byteCount)
    {
        using RentedArray<byte> sourceBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);
        using RentedArray<byte> destinationBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);

        Span<byte> source = sourceBuffer[..byteCount];

        int elementsToSkip = Random.Shared.Next(1, 64);
        source = source[elementsToSkip..]; // destroy alignment

        Random.Shared.Fill(source);
        Span<byte> destination = destinationBuffer[..byteCount];
        destination = destination[elementsToSkip..];

        SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);
        Assert.True(destination.SequenceEqual(source));
    }

    [Theory]
    [MemberData(nameof(MemmoveUnalignedAndOverlappingParameters))]
    public void Memmove_Overlapping_DestinationGreaterThanSource_Test(int byteCount)
    {
        using RentedArray<byte> sourceBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);

        Span<byte> source = sourceBuffer[..byteCount];
        Random.Shared.Fill(source);

        int elementsToSkip = Random.Shared.Next(1, byteCount / 8);
        Span<byte> destination = source[elementsToSkip..];

        Span<byte> expectedItems = source[..destination.Length].ToArray();
        SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)destination.Length);
        Assert.True(destination.SequenceEqual(expectedItems));
    }

    [Theory]
    [MemberData(nameof(MemmoveUnalignedAndOverlappingParameters))]
    public void Memmove_Overlapping_SourceGreaterThanDestination_Test(int byteCount)
    {
        using RentedArray<byte> sourceBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(byteCount);

        Span<byte> source = sourceBuffer[..byteCount];
        Random.Shared.Fill(source);

        Span<byte> destination = source;
        int elementsToSkip = Random.Shared.Next(1, byteCount / 8);
        source = source[elementsToSkip..];

        Span<byte> expectedItems = source.ToArray();
        SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);
        Assert.True(destination[..source.Length].SequenceEqual(expectedItems));
    }

    private static TheoryData<int> CreateMemmoveParameters()
    {
        const int MaxElementCount = ushort.MaxValue * 2;

        TheoryData<int> data = new();
        for (int i = 0; i <= MaxElementCount; i++)
        {
            data.Add(i);
        }

        for (int i = 1; i <= 6; i++)
        {
            data.Add(0xFFFF << i);
        }

        return data;
    }
}
