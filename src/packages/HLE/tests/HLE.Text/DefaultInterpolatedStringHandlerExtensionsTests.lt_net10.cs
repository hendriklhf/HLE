using System;
using System.Runtime.CompilerServices;
using HLE;
using HLE.Text;

namespace HLE.UnitTests.HLE.Text;

public sealed class DefaultInterpolatedStringHandlerExtensionsTests
{
    [Fact]
    public void GetText()
    {
        DefaultInterpolatedStringHandler handler = new();
        handler.AppendLiteral("hello");
        handler.AppendFormatted(" world");

        Assert.True(handler.Text.SequenceEqual("hello world"));

        handler.Clear();
    }

    [Fact]
    public void Clear()
    {
        DefaultInterpolatedStringHandler handler = new();
        handler.AppendLiteral("hello");
        handler.AppendFormatted(" world");

        handler.Clear();

        Assert.Same(string.Empty, handler.ToString());
        Assert.Equal(0, handler.Text.Length);
    }
}
