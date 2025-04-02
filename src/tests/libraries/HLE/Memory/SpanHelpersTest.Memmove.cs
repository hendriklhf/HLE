using System;
using System.Runtime.InteropServices;
using HLE.Memory;
using Xunit;

namespace HLE.UnitTests.Memory;


public sealed partial class SpanHelpersTest
{
    [Theory]
    public void Memmove_Int8_ElementCount(sbyte elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_UInt8_ElementCount(byte elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_Int16_ElementCount(short elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_UInt16_ElementCount(ushort elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_Int32_ElementCount(int elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_UInt32_ElementCount(uint elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_Int64_ElementCount(long elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_UInt64_ElementCount(ulong elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_IntPtr_ElementCount(nint elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }

    [Theory]
    public void Memmove_UIntPtr_ElementCount(nuint elementCount)
    {
        byte[] source = new byte[elementCount];
        Random.Shared.Fill(source);
        byte[] destination = new byte[elementCount];
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), elementCount);
        Assert.True(source.AsSpan().SequenceEqual(destination.AsSpan()));
    }
}
