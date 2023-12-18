using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using Xunit;

namespace HLE.Tests;

public sealed class BufferedFileReaderTest
{
    private readonly string _temporaryDirectory;

    public BufferedFileReaderTest()
    {
        string temporaryDirectory = $"{Path.GetTempPath()}{nameof(BufferedFileReaderTest)}_{Guid.NewGuid():N}";
        Directory.CreateDirectory(temporaryDirectory);
        _temporaryDirectory = temporaryDirectory;
    }

    private string WriteFileAndGetPath(string content)
    {
        string path = $"{_temporaryDirectory}{Path.DirectorySeparatorChar}{Guid.NewGuid():N}";
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
        using PooledBufferWriter<char> bytes = new();
        reader.ReadChars(bytes, Encoding.UTF8);

        Assert.True(bytes.WrittenSpan is "hello");
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
        using PooledBufferWriter<char> bytes = new();
        await reader.ReadCharsAsync(bytes, Encoding.UTF8);

        Assert.True(bytes.WrittenSpan is "hello");
    }
}
