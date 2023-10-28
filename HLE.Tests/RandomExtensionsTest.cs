using System;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace HLE.Tests;

public sealed class RandomExtensionsTest
{
    private const int _loopIterations = 2048;

    [Fact]
    public void CharTest()
    {
        const char min = (char)32;
        const char max = (char)127;
        for (int i = 0; i < _loopIterations; i++)
        {
            char c = Random.Shared.NextChar(min, max);
            Assert.True(c is >= min and < max);
        }
    }

    [Fact]
    public void NextStringTest()
    {
        const int stringLength = 0x1000;
        for (int i = 0; i < _loopIterations; i++)
        {
            string str = Random.Shared.NextString(stringLength);
            Assert.Equal(stringLength, str.Length);
        }
    }

    [Fact]
    public void NextString_Max_Test()
    {
        const char max = (char)127;
        const int stringLength = 0x1000;
        for (int i = 0; i < _loopIterations; i++)
        {
            string str = Random.Shared.NextString(stringLength, max);
            Assert.Equal(stringLength, str.Length);
            Assert.True(str.All(static c => c < max));
        }
    }

    [Fact]
    public void NextString_Min_Max_Test()
    {
        const char min = (char)32;
        const char max = (char)127;
        const int stringLength = 0x1000;
        for (int i = 0; i < _loopIterations; i++)
        {
            string str = Random.Shared.NextString(stringLength, min, max);
            Assert.Equal(stringLength, str.Length);
            Assert.True(str.All(static c => c is >= min and < max));
        }
    }

    [Fact]
    public void NextString_FromChars_Test()
    {
        const int stringLength = 0x1000;
        const string chars = "hello";
        for (int i = 0; i < _loopIterations; i++)
        {
            string s = Random.Shared.NextString(stringLength, chars);
            Assert.Equal(stringLength, s.Length);
            Assert.True(s.All(static c => chars.Contains(c)));
        }
    }

    [Fact]
    public void ByteTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(Random.Shared.NextUInt8(20, 150) is >= 20 and < 150);
        }
    }

    [Fact]
    public void SByteTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(Random.Shared.NextInt8(-50, 120) is >= -50 and < 120);
        }
    }

    [Fact]
    public void ShortTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(Random.Shared.NextInt16(-50, 20200) is >= -50 and < 20200);
        }
    }

    [Fact]
    public void UShortTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(Random.Shared.NextUInt16(1000, 55555) is >= 1000 and < 55555);
        }
    }

    [Fact]
    public void UIntTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(Random.Shared.NextUInt32(100_000, 4_000_000) is >= 100_000 and < 4_000_000);
        }
    }

    [Fact]
    public void StrongByteTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetUInt8(55, 220) is >= 55 and < 220);
        }
    }

    [Fact]
    public void StrongSByteTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetInt8(-120, 5) is >= -120 and < 5);
        }
    }

    [Fact]
    public void StrongShortTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetInt16(-20_000, 20000) is >= -20_000 and < 20_000);
        }
    }

    [Fact]
    public void StrongUShortTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetUInt16(20_000, 60_000) is >= 20_000 and < 60_000);
        }
    }

    [Fact]
    public void StrongIntTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetInt32(-2_000_000, 1000) is >= -2_000_000 and < 1000);
        }
    }

    [Fact]
    public void StrongUIntTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetUInt32(20_000_000) is >= 20_000_000 and < uint.MaxValue);
        }
    }

    [Fact]
    public void StrongLongTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            long value = rng.GetInt64(-500, 500);
            Assert.True(value is >= -500 and < 500);
        }
    }

    [Fact]
    public void StrongULongTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetUInt64(0, 50) < 50);
        }
    }

    [Fact]
    public void StrongCharTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.True(rng.GetChar((char)32, (char)127) is >= (char)32 and < (char)127);
        }
    }

    [Fact]
    public void StrongStringTest()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        for (int i = 0; i < _loopIterations; i++)
        {
            string str = rng.GetString(50);
            Assert.Equal(50, str.Length);
        }
    }
}
