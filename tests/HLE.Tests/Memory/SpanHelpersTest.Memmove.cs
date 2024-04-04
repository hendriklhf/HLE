using System;
using System.Runtime.InteropServices;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed partial class SpanHelpersTest
{
    public static TheoryData<int> MemmoveParameters { get; } = CreateMemmoveParameters();

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

    [Fact]
    public void MemmoveUnaligned_RandomSizeIterations_Test()
    {
        for (int i = 0; i < 1024; i++)
        {
            int elementCount = Random.Shared.Next(64, 1048576);
            Span<byte> source = GC.AllocateUninitializedArray<byte>(elementCount);

            int elementsToSkip = Random.Shared.Next(1, 64);
            source = source[elementsToSkip..]; // destroy alignment

            Random.Shared.Fill(source);
            Span<byte> destination = GC.AllocateUninitializedArray<byte>(elementCount);
            destination = destination[elementsToSkip..];

            SpanHelpers<byte>.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);
            Assert.True(destination.SequenceEqual(source));
        }
    }

    private static TheoryData<int> CreateMemmoveParameters()
    {
        TheoryData<int> data = new();
        for (int i = 0; i <= 2048; i++)
        {
            data.Add(i);
        }

        for (int i = 0; i < 128; i++)
        {
            data.Add(Random.Shared.Next(4096, 1048576));
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
