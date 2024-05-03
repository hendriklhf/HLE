using System;
using System.Runtime.InteropServices;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed partial class SpanHelpersTest
{
    public static TheoryData<int> MemmoveParameters { get; } = CreateMemmoveParameters(0, 2048);

    public static TheoryData<int> MemmoveUnalignedAndOverlappingParameters { get; } = CreateMemmoveParameters(64, 2048);

    [Theory]
    [MemberData(nameof(MemmoveParameters))]
    public void MemmoveByteTest(int byteCount)
    {
        Span<byte> source = GC.AllocateUninitializedArray<byte>(byteCount);
        Random.Shared.Fill(source);
        Span<byte> destination = GC.AllocateUninitializedArray<byte>(byteCount);

        SpanHelpers<byte>.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);
        Assert.True(destination.SequenceEqual(source));
    }

    [Theory]
    [MemberData(nameof(MemmoveParameters))]
    public void MemmoveStringTest(int stringCount)
    {
        Span<string> source = GC.AllocateUninitializedArray<string>(stringCount);
        FillWithRandomStrings(source);
        Span<string> destination = new string[stringCount];

        SpanHelpers<string>.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

        for (int i = 0; i < source.Length; i++)
        {
            Assert.Same(source[i], destination[i]);
        }
    }

    [Theory]
    [MemberData(nameof(MemmoveUnalignedAndOverlappingParameters))]
    public void Memmove_Unaligned_Test(int byteCount)
    {
        Span<byte> source = GC.AllocateUninitializedArray<byte>(byteCount);

        int elementsToSkip = Random.Shared.Next(1, 64);
        source = source[elementsToSkip..]; // destroy alignment

        Random.Shared.Fill(source);
        Span<byte> destination = GC.AllocateUninitializedArray<byte>(byteCount);
        destination = destination[elementsToSkip..];

        SpanHelpers<byte>.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);
        Assert.True(destination.SequenceEqual(source));
    }

    [Theory]
    [MemberData(nameof(MemmoveUnalignedAndOverlappingParameters))]
    public void Memmove_Overlapping_Test(int byteCount)
    {
        Span<byte> source = GC.AllocateUninitializedArray<byte>(byteCount);

        Random.Shared.Fill(source);
        int elementsToSkip = Random.Shared.Next(1, 64);
        Span<byte> destination = source[elementsToSkip..];

        Span<byte> expectedItems = source[..destination.Length].ToArray();
        SpanHelpers<byte>.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)destination.Length);
        Assert.True(destination.SequenceEqual(expectedItems));
    }

    private static TheoryData<int> CreateMemmoveParameters(int minLength, int maxLength)
    {
        TheoryData<int> data = new();
        for (int i = minLength; i <= maxLength; i++)
        {
            data.Add(i);
        }

        for (int i = 0; i < 128; i++)
        {
            data.Add(Random.Shared.Next(4096, 1_048_576));
        }

        return data;
    }

    private static void FillWithRandomStrings(Span<string> strings)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            strings[i] = Random.Shared.NextString(8);
        }
    }
}
