using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class BufferedFileReaderBufferedFileWriterTest
{
    private static readonly string s_tempFileDirectory = $"{Path.GetTempPath()}HLE.Tests.BufferedFileOperationsTest\\";

    [TestInitialize]
    public void Initialize() => Directory.CreateDirectory(s_tempFileDirectory);

    [TestCleanup]
    public void Cleanup() => Directory.Delete(s_tempFileDirectory, true);

    private static string CreateFile(string fileContent, Encoding fileEncoding)
    {
        string filePath = $"{s_tempFileDirectory}{Guid.NewGuid():N}";
        byte[] fileContentBytes = fileEncoding.GetBytes(fileContent);
        File.WriteAllBytes(filePath, fileContentBytes);
        return filePath;
    }

    [TestMethod]
    public void ReadBytesTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<byte> writer = new(10);
        new BufferedFileReader(filePath).ReadBytes(writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello".AsByteSpan()));

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        new BufferedFileReader(filePath).ReadBytes(writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public async Task ReadBytesAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<byte> writer = new(10);
        await new BufferedFileReader(filePath).ReadBytesAsync(writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello".AsByteSpan()));

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        await new BufferedFileReader(filePath).ReadBytesAsync(writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public void ReadCharsTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<char> writer = new(10);
        new BufferedFileReader(filePath).ReadChars(writer, Encoding.Unicode);
        Assert.IsTrue(writer.WrittenSpan is "hello");

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        new BufferedFileReader(filePath).ReadChars(writer, Encoding.UTF8);
        Assert.IsTrue(writer.WrittenSpan is "hello");
    }

    [TestMethod]
    public async Task ReadCharsAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PooledBufferWriter<char> writer = new(10);
        await new BufferedFileReader(filePath).ReadCharsAsync(writer, Encoding.Unicode);
        Assert.IsTrue(writer.WrittenSpan is "hello");

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        await new BufferedFileReader(filePath).ReadCharsAsync(writer, Encoding.UTF8);
        Assert.IsTrue(writer.WrittenSpan is "hello");
    }

    [TestMethod]
    public void WriteBytesTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteBytes("hello"u8);
        ReadOnlySpan<byte> fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteBytes("hello"u8);
        fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public async Task WriteBytesAsync_Test_Async()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteBytesAsync("hello"u8.ToArray());
        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteBytesAsync("hello"u8.ToArray());
        fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hello"u8));
    }

    [TestMethod]
    public void WriteCharsTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteChars("hello", Encoding.UTF8);
        ReadOnlySpan<char> fileContent = File.ReadAllText(filePath);
        Assert.IsTrue(fileContent is "hello");

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).WriteChars("hello", Encoding.UTF8);
        fileContent = File.ReadAllText(filePath);
        Assert.IsTrue(fileContent is "hello");
    }

    [TestMethod]
    public async Task WriteCharsAsync_Test_Async()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteCharsAsync("hello".AsMemory(), Encoding.UTF8);
        string fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).WriteCharsAsync("hello".AsMemory(), Encoding.UTF8);
        fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hello", fileContent);
    }

    [TestMethod]
    public void AppendBytesTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendBytes("hello"u8);
        ReadOnlySpan<byte> fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hellohello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendBytes("hello"u8);
        fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public async Task AppendBytesAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendBytesAsync("hello"u8.ToArray());
        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hellohello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendBytesAsync("hello"u8.ToArray());
        fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hello"u8));
    }

    [TestMethod]
    public void AppendCharsTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendChars("hello", Encoding.UTF8);
        string fileContent = File.ReadAllText(filePath);
        Assert.AreEqual("hellohello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        new BufferedFileWriter(filePath).AppendChars("hello", Encoding.UTF8);
        fileContent = File.ReadAllText(filePath);
        Assert.AreEqual("hello", fileContent);
    }

    [TestMethod]
    public async Task AppendCharsAsync_Test_Async()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendCharsAsync("hello".AsMemory(), Encoding.UTF8);
        string fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hellohello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await new BufferedFileWriter(filePath).AppendCharsAsync("hello".AsMemory(), Encoding.UTF8);
        fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hello", fileContent);
    }
}
