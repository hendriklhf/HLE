using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.IO;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.IO;

public sealed class BufferedFileReaderTest
{
    private readonly string _temporaryDirectory;

    public BufferedFileReaderTest()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), PathHelpers.TypeNameToPath<BufferedFileReaderTest>());
        Directory.CreateDirectory(temporaryDirectory);
        _temporaryDirectory = temporaryDirectory;
    }

    private string WriteFileAndGetPath(string content)
    {
        string path = Path.Combine(_temporaryDirectory, $"{Guid.NewGuid():N}");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void ReadBytesTest()
    {
        string path = WriteFileAndGetPath("hello");

        using BufferedFileReader reader = new(path);
        using PooledBufferWriter<byte> bytes = new();
        reader.ReadBytes(bytes);

        Assert.True(bytes.WrittenSpan.SequenceEqual("hello"u8));
    }

    [Fact]
    public void ReadCharsTest()
    {
        string path = WriteFileAndGetPath("hello");

        using BufferedFileReader reader = new(path);
        using PooledBufferWriter<char> chars = new();
        reader.ReadChars(chars, Encoding.UTF8);

        Assert.True(chars.WrittenSpan is "hello");
    }

    [Fact]
    public async Task ReadBytesAsyncTestAsync()
    {
        string path = WriteFileAndGetPath("hello");

        using BufferedFileReader reader = new(path);
        using PooledBufferWriter<byte> bytes = new();
        await reader.ReadBytesAsync(bytes);

        Assert.True(bytes.WrittenSpan.SequenceEqual("hello"u8));
    }

    [Fact]
    public async Task ReadCharsAsyncTestAsync()
    {
        string path = WriteFileAndGetPath("hello");

        using BufferedFileReader reader = new(path);
        using PooledBufferWriter<char> chars = new();
        await reader.ReadCharsAsync(chars, Encoding.UTF8);

        Assert.True(chars.WrittenSpan is "hello");
    }
}
