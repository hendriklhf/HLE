using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Strings;
using Xunit;

namespace HLE.Tests;

public sealed class BufferedFileReaderBufferedFileWriterTest : IDisposable
{
    private static readonly string s_temporaryDirectory = $"{Path.GetTempPath()}HLE.Tests.{nameof(BufferedFileReaderBufferedFileWriterTest)}\\";

    public BufferedFileReaderBufferedFileWriterTest() => Directory.CreateDirectory(s_temporaryDirectory);

    public void Dispose() => Directory.Delete(s_temporaryDirectory, true);

    private static string CreateFile(string fileContent, Encoding fileEncoding)
    {
        string filePath = $"{s_temporaryDirectory}{Guid.NewGuid():N}";
        byte[] fileContentBytes = fileEncoding.GetBytes(fileContent);
        File.WriteAllBytes(filePath, fileContentBytes);
        return filePath;
    }

    [Fact]
    public void ReadBytesTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<byte> writer = new(10);
        new BufferedFileReader(filePath).ReadBytes(writer);
        Assert.True(writer.WrittenSpan.SequenceEqual("hello".AsByteSpan()));

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        new BufferedFileReader(filePath).ReadBytes(writer);
        Assert.True(writer.WrittenSpan.SequenceEqual("hello"u8));
    }

    [Fact]
    public async Task ReadBytesAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<byte> writer = new(10);
        await new BufferedFileReader(filePath).ReadBytesAsync(writer);
        Assert.True(writer.WrittenSpan.SequenceEqual("hello".AsByteSpan()));

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        await new BufferedFileReader(filePath).ReadBytesAsync(writer);
        Assert.True(writer.WrittenSpan.SequenceEqual("hello"u8));
    }

    [Fact]
    public void ReadCharsTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<char> writer = new(10);
        new BufferedFileReader(filePath).ReadChars(writer, Encoding.Unicode);
        Assert.True(writer.WrittenSpan is "hello");

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        new BufferedFileReader(filePath).ReadChars(writer, Encoding.UTF8);
        Assert.True(writer.WrittenSpan is "hello");
    }

    [Fact]
    public async Task ReadCharsAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<char> writer = new(10);
        await new BufferedFileReader(filePath).ReadCharsAsync(writer, Encoding.Unicode);
        Assert.True(writer.WrittenSpan is "hello");

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        await new BufferedFileReader(filePath).ReadCharsAsync(writer, Encoding.UTF8);
        Assert.True(writer.WrittenSpan is "hello");
    }

    [Fact]
    public void WriteBytesTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteBytes("hello"u8);
        ReadOnlySpan<byte> fileContent = File.ReadAllBytes(filePath);
        Assert.True(fileContent.SequenceEqual("hello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteBytes("hello"u8);
        fileContent = File.ReadAllBytes(filePath);
        Assert.True(fileContent.SequenceEqual("hello"u8));
    }

    [Fact]
    public async Task WriteBytesAsync_Test_Async()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteBytesAsync("hello"u8.ToArray());
        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.True(fileContent.AsSpan().SequenceEqual("hello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteBytesAsync("hello"u8.ToArray());
        fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.True(fileContent.AsSpan().SequenceEqual("hello"u8));
    }

    [Fact]
    public void WriteCharsTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteChars("hello", Encoding.UTF8);
        ReadOnlySpan<char> fileContent = File.ReadAllText(filePath);
        Assert.True(fileContent is "hello");

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteChars("hello", Encoding.UTF8);
        fileContent = File.ReadAllText(filePath);
        Assert.True(fileContent is "hello");
    }

    [Fact]
    public async Task WriteCharsAsync_Test_Async()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteCharsAsync("hello".AsMemory(), Encoding.UTF8);
        string fileContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal("hello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteCharsAsync("hello".AsMemory(), Encoding.UTF8);
        fileContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal("hello", fileContent);
    }

    [Fact]
    public void AppendBytesTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendBytes("hello"u8);
        ReadOnlySpan<byte> fileContent = File.ReadAllBytes(filePath);
        Assert.True(fileContent.SequenceEqual("hellohello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendBytes("hello"u8);
        fileContent = File.ReadAllBytes(filePath);
        Assert.True(fileContent.SequenceEqual("hello"u8));
    }

    [Fact]
    public async Task AppendBytesAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendBytesAsync("hello"u8.ToArray());
        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.True(fileContent.AsSpan().SequenceEqual("hellohello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendBytesAsync("hello"u8.ToArray());
        fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.True(fileContent.AsSpan().SequenceEqual("hello"u8));
    }

    [Fact]
    public void AppendCharsTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendChars("hello", Encoding.UTF8);
        string fileContent = File.ReadAllText(filePath);
        Assert.Equal("hellohello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendChars("hello", Encoding.UTF8);
        fileContent = File.ReadAllText(filePath);
        Assert.Equal("hello", fileContent);
    }

    [Fact]
    public async Task AppendCharsAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendCharsAsync("hello".AsMemory(), Encoding.UTF8);
        string fileContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal("hellohello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendCharsAsync("hello".AsMemory(), Encoding.UTF8);
        fileContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal("hello", fileContent);
    }
}
