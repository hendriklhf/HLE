using System;
using System.Runtime.CompilerServices;
using HLE.Marshalling;
using HLE.TestUtilities;

namespace HLE.UnitTests.Marshalling;

public sealed unsafe class StringAllocatorTests
{
    public static TheoryData<int> MisalignmentParameters { get; } = TheoryDataHelpers.CreateRange(1, sizeof(nuint) - 1);

    [Theory]
    [MemberData(nameof(MisalignmentParameters))]
    public void Alloc_ThrowsArgumentException_WhenBufferIsNotAligned(int misalignment)
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
        {
            Span<byte> buffer = stackalloc byte[64];
            buffer = buffer[misalignment..];
            ref RawStringData str = ref StringAllocator.Alloc(buffer, "test");
            TestHelpers.Consume(str);
        });

        Assert.Contains("aligned", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Alloc_ThrowsArgumentOutOfRangeException_WhenBufferIsTooSmall()
    {
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            ReadOnlySpan<char> chars = "Hello World!";
            int requiredSize = int.CreateChecked(ObjectMarshal.GetRawStringSize(chars.Length));
            Span<byte> buffer = stackalloc byte[requiredSize - 1];
            ref RawStringData str = ref StringAllocator.Alloc(buffer, chars);
            TestHelpers.Consume(str);
        });

        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Alloc_CreatesStringCorrectly_WhenBufferIsLargerThanNeeded()
    {
        ReadOnlySpan<char> chars = "Hello World!";
        Span<byte> buffer = stackalloc byte[sizeof(char) * chars.Length + 64];
        ref RawStringData str = ref StringAllocator.Alloc(buffer, chars);
        string result = ObjectMarshal.GetString(ref str);

        Assert.Equal(0U, Unsafe.Add(ref Unsafe.As<RawStringData, nuint>(ref str), -1));
        Assert.Equal(typeof(string), result.GetType());
        Assert.Equal(chars.Length, result.Length);
        Assert.True(chars.SequenceEqual(result));
    }

    [Fact]
    public void Alloc_CreatesStringCorrectly_WhenBufferIsExactlyAsLargeAsNeeded()
    {
        ReadOnlySpan<char> chars = "Hello World!";
        int size = int.CreateChecked(ObjectMarshal.GetRawStringSize(chars.Length));
        Span<byte> buffer = stackalloc byte[size];
        ref RawStringData str = ref StringAllocator.Alloc(buffer, chars);
        string result = ObjectMarshal.GetString(ref str);

        Assert.Equal(0U, Unsafe.Add(ref Unsafe.As<RawStringData, nuint>(ref str), -1));
        Assert.Equal(typeof(string), result.GetType());
        Assert.Equal(chars.Length, result.Length);
        Assert.True(chars.SequenceEqual(result));
    }
}
