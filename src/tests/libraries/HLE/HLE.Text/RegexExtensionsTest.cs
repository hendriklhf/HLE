using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HLE.IO;
using Xunit;

namespace HLE.Text.UnitTests;

public sealed partial class RegexExtensionsTest
{
    [GeneratedRegex(@"\d+", RegexOptions.Compiled)]
    public static partial Regex NumbersPattern { get; }

    [Fact]
    public void IsMatch()
    {
        bool isMatch = NumbersPattern.IsMatch("hello123hello123hello"u8, Encoding.UTF8);
        bool expectedIsMatch = NumbersPattern.IsMatch("hello123hello123hello");
        Assert.Equal(expectedIsMatch, isMatch);

        isMatch = NumbersPattern.IsMatch("hellohellohello"u8, Encoding.UTF8);
        expectedIsMatch = NumbersPattern.IsMatch("hellohellohello");
        Assert.Equal(expectedIsMatch, isMatch);
    }

    [Fact]
    public void Count()
    {
        int count = NumbersPattern.Count("hello123hello123hello"u8, Encoding.UTF8);
        int expectedCount = NumbersPattern.Count("hello123hello123hello");
        Assert.Equal(expectedCount, count);

        count = NumbersPattern.Count("hellohellohello"u8, Encoding.UTF8);
        expectedCount = NumbersPattern.Count("hellohellohello");
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public async Task IsMatchAsync()
    {
        {
            await using PooledMemoryStream stream = new("hello123hello123hello"u8);
            stream.Position = 0;
            bool isMatch = await NumbersPattern.IsMatchAsync(stream, Encoding.UTF8);
            bool expectedIsMatch = NumbersPattern.IsMatch("hello123hello123hello");
            Assert.Equal(expectedIsMatch, isMatch);
        }

        {
            await using PooledMemoryStream stream = new("hellohellohello"u8);
            stream.Position = 0;
            bool isMatch = await NumbersPattern.IsMatchAsync(stream, Encoding.UTF8);
            bool expectedIsMatch = NumbersPattern.IsMatch("hellohellohello");
            Assert.Equal(expectedIsMatch, isMatch);
        }
    }

    [Fact]
    public async Task CountAsync()
    {
        {
            await using PooledMemoryStream stream = new("hello123hello123hello"u8);
            stream.Position = 0;
            int count = await NumbersPattern.CountAsync(stream, Encoding.UTF8);
            int expectedCount = NumbersPattern.Count("hello123hello123hello");
            Assert.Equal(expectedCount, count);
        }

        {
            await using PooledMemoryStream stream = new("hellohellohello"u8);
            stream.Position = 0;
            int count = await NumbersPattern.CountAsync(stream, Encoding.UTF8);
            int expectedCount = NumbersPattern.Count("hellohellohello");
            Assert.Equal(expectedCount, count);
        }
    }
}
