using System;
using System.Runtime.InteropServices;

namespace HLE.Memory.UnitTests;

public sealed partial class SpanHelpersTest
{
    public static TheoryData<sbyte> MemmoveInt8ElementCounts { get; } = [0, 26, 99, sbyte.MaxValue];

    public static TheoryData<byte> MemmoveUInt8ElementCounts { get; } = [0, 26, 99, 201, byte.MaxValue];

    public static TheoryData<short> MemmoveInt16ElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, short.MaxValue];

    public static TheoryData<ushort> MemmoveUInt16ElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, 41_295, ushort.MaxValue];

    public static TheoryData<int> MemmoveInt32ElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, 41_295, 537_542, 3_453_435, 8_438_139];

    public static TheoryData<uint> MemmoveUInt32ElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, 41_295, 537_542, 3_453_435, 8_438_139];

    public static TheoryData<long> MemmoveInt64ElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, 41_295, 537_542, 3_453_435, 8_438_139];

    public static TheoryData<ulong> MemmoveUInt64ElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, 41_295, 537_542, 3_453_435, 8_438_139];

    public static TheoryData<nint> MemmoveIntPtrElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, 41_295, 537_542, 3_453_435, 8_438_139];

    public static TheoryData<nuint> MemmoveUIntPtrElementCounts { get; } = [0, 26, 99, 201, 943, 7_459, 17_674, 41_295, 537_542, 3_453_435, 8_438_139];

    [Theory]
    [MemberData(nameof(MemmoveInt8ElementCounts))]
    public void Memmove_Int8_ElementCount(sbyte elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveUInt8ElementCounts))]
    public void Memmove_UInt8_ElementCount(byte elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveInt16ElementCounts))]
    public void Memmove_Int16_ElementCount(short elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveUInt16ElementCounts))]
    public void Memmove_UInt16_ElementCount(ushort elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveInt32ElementCounts))]
    public void Memmove_Int32_ElementCount(int elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveUInt32ElementCounts))]
    public void Memmove_UInt32_ElementCount(uint elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveInt64ElementCounts))]
    public void Memmove_Int64_ElementCount(long elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveUInt64ElementCounts))]
    public void Memmove_UInt64_ElementCount(ulong elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveIntPtrElementCounts))]
    public void Memmove_IntPtr_ElementCount(nint elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(MemmoveUIntPtrElementCounts))]
    public void Memmove_UIntPtr_ElementCount(nuint elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }
}
