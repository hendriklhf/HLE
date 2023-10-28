using System;
using HLE.Strings;
using Xunit;

namespace HLE.Tests.Strings;

public sealed class PooledStringTest
{
    [Fact]
    public void CreateFromLengthTest()
    {
        for (int length = 0; length <= 10_000; length *= 10)
        {
            using PooledString pooledString = new(length);

            Assert.Equal(length, pooledString.Length);
            Assert.Equal(length, pooledString.AsString().Length);
            Assert.Equal(length, pooledString.AsSpan().Length);

            Assert.True(pooledString.AsSpan().SequenceEqual(pooledString.AsString()));

            if (length == 0)
            {
                length = 1;
            }
        }
    }

    [Fact]
    public void CreateFromSpanTest()
    {
        for (int length = 0; length <= 10_000; length *= 10)
        {
            ReadOnlySpan<char> span = Random.Shared.NextString(length);
            using PooledString pooledString = new(span);

            Assert.Equal(span.Length, pooledString.Length);
            Assert.Equal(span.Length, pooledString.AsString().Length);
            Assert.Equal(span.Length, pooledString.AsSpan().Length);

            Assert.True(pooledString.AsSpan().SequenceEqual(pooledString.AsString()));
            Assert.True(span.SequenceEqual(pooledString.AsString()));

            if (length == 0)
            {
                length = 1;
            }
        }
    }
}
