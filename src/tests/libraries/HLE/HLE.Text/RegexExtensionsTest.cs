using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HLE.IO;

namespace HLE.Text.UnitTests;

public sealed partial class RegexExtensionsTest
{
    [GeneratedRegex(@"\d+", RegexOptions.Compiled)]
    public static partial Regex GetNumbersPattern();

    [Fact]
    public void IsMatch()
    {
        bool isMatch = GetNumbersPattern().IsMatch("hello123hello123hello"u8, Encoding.UTF8);
        bool expectedIsMatch = GetNumbersPattern().IsMatch("hello123hello123hello");
        Assert.Equal(expectedIsMatch, isMatch);

        isMatch = GetNumbersPattern().IsMatch("hellohellohello"u8, Encoding.UTF8);
        expectedIsMatch = GetNumbersPattern().IsMatch("hellohellohello");
        Assert.Equal(expectedIsMatch, isMatch);
    }

    [Fact]
    public void Count()
    {
        int count = GetNumbersPattern().Count("hello123hello123hello"u8, Encoding.UTF8);
        int expectedCount = GetNumbersPattern().Count("hello123hello123hello");
        Assert.Equal(expectedCount, count);

        count = GetNumbersPattern().Count("hellohellohello"u8, Encoding.UTF8);
        expectedCount = GetNumbersPattern().Count("hellohellohello");
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public async Task IsMatchAsync()
    {
        {
            await using PooledMemoryStream stream = new("hello123hello123hello"u8);
            stream.Position = 0;
            bool isMatch = await GetNumbersPattern().IsMatchAsync(stream, Encoding.UTF8);
            bool expectedIsMatch = GetNumbersPattern().IsMatch("hello123hello123hello");
            Assert.Equal(expectedIsMatch, isMatch);
        }

        {
            await using PooledMemoryStream stream = new("hellohellohello"u8);
            stream.Position = 0;
            bool isMatch = await GetNumbersPattern().IsMatchAsync(stream, Encoding.UTF8);
            bool expectedIsMatch = GetNumbersPattern().IsMatch("hellohellohello");
            Assert.Equal(expectedIsMatch, isMatch);
        }
    }

    [Fact]
    public async Task CountAsync()
    {
        {
            await using PooledMemoryStream stream = new("hello123hello123hello"u8);
            stream.Position = 0;
            int count = await GetNumbersPattern().CountAsync(stream, Encoding.UTF8);
            int expectedCount = GetNumbersPattern().Count("hello123hello123hello");
            Assert.Equal(expectedCount, count);
        }

        {
            await using PooledMemoryStream stream = new("hellohellohello"u8);
            stream.Position = 0;
            int count = await GetNumbersPattern().CountAsync(stream, Encoding.UTF8);
            int expectedCount = GetNumbersPattern().Count("hellohellohello");
            Assert.Equal(expectedCount, count);
        }
    }
}
