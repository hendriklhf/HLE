using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using Xunit;

namespace HLE.Tests.Marshalling;

public sealed class StringMarshalTest
{
    [Fact]
    public void FastAllocateString_Test()
    {
        string str = StringMarshal.FastAllocateString(5, out Span<char> chars);
        "hello".CopyTo(chars);
        Assert.Equal("hello", str);
        Assert.Equal("hello".Length, str.Length);
        Assert.NotSame("hello", str);
    }

    [Fact]
    public void FastAllocateString_Empty_Test()
    {
        string str = StringMarshal.FastAllocateString(0, out Span<char> chars);
        Assert.Same(string.Empty, str);
        Assert.Equal(0, chars.Length);
    }

    [Fact]
    public void FastAllocateString_ThrowsArgumentOutOfRangeException_Test() =>
        Assert.Throws<ArgumentOutOfRangeException>(static () => _ = StringMarshal.FastAllocateString(-1, out _));

    [Fact]
    public void AsMutableSpanTest()
    {
        const string Str = "hello";
        Span<char> chars = StringMarshal.AsMutableSpan(Str);
        Assert.True(chars is "hello");
    }

    [Fact]
    public void AsStringTest()
    {
        ReadOnlySpan<char> span = "hello";
        string str = StringMarshal.AsString(span);
        Assert.True(span.SequenceEqual(str));

        ref char spanRef = ref MemoryMarshal.GetReference(span);
        ref char strRef = ref MemoryMarshal.GetReference(str.AsSpan());
        Assert.True(Unsafe.AreSame(ref spanRef, ref strRef));
    }

    [Fact]
    public void GetReference_Test()
    {
        const string Hello = "hello";
        ref char reference = ref StringMarshal.GetReference(Hello);
        Assert.True(Unsafe.AreSame(ref reference, ref MemoryMarshal.GetReference(Hello.AsSpan())));
    }
}
