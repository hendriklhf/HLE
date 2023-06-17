using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class BufferedFileOperationsTest
{
    private static readonly string _tempFileDirectory = $"{Path.GetTempPath()}HLE.Tests.FilesTest\\";

    [TestInitialize]
    public void Setup()
    {
        Directory.CreateDirectory(_tempFileDirectory);
    }

    [TestCleanup]
    public void Dispose()
    {
        Directory.Delete(_tempFileDirectory, true);
    }

    private static string CreateFile(string fileContent, Encoding fileEncoding)
    {
        string filePath = $"{_tempFileDirectory}{Guid.NewGuid():N}";
        byte[] fileContentBytes = fileEncoding.GetBytes(fileContent);
        File.WriteAllBytes(filePath, fileContentBytes);
        return filePath;
    }

    [TestMethod]
    public void ReadBytesTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PoolBufferWriter<byte> writer = new(10, 5);
        BufferedFileOperations.ReadBytes(filePath, writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello".AsByteSpan()));

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        BufferedFileOperations.ReadBytes(filePath, writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public async Task ReadBytesAsyncTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PoolBufferWriter<byte> writer = new(10, 5);
        await BufferedFileOperations.ReadBytesAsync(filePath, writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello".AsByteSpan()));

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        await BufferedFileOperations.ReadBytesAsync(filePath, writer);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public void ReadCharsTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PoolBufferWriter<char> writer = new(10, 5);
        BufferedFileOperations.ReadChars(filePath, Encoding.Unicode, writer, 10);
        Assert.IsTrue(writer.WrittenSpan is "hello");

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        BufferedFileOperations.ReadChars(filePath, Encoding.UTF8, writer, 10);
        Assert.IsTrue(writer.WrittenSpan is "hello");
    }

    [TestMethod]
    public async Task ReadCharsAsyncTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        using PoolBufferWriter<char> writer = new(10, 5);
        await BufferedFileOperations.ReadCharsAsync(filePath, Encoding.Unicode, writer, 10);
        Assert.IsTrue(writer.WrittenSpan is "hello");

        filePath = CreateFile("hello", Encoding.UTF8);
        writer.Clear();
        await BufferedFileOperations.ReadCharsAsync(filePath, Encoding.UTF8, writer, 10);
        Assert.IsTrue(writer.WrittenSpan is "hello");
    }

    [TestMethod]
    public void ReadStringTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        string fileContent = BufferedFileOperations.ReadString(filePath, Encoding.Unicode, 10);
        Assert.AreEqual("hello", fileContent);

        filePath = CreateFile("hello", Encoding.UTF8);
        fileContent = BufferedFileOperations.ReadString(filePath, Encoding.UTF8, 10);
        Assert.AreEqual("hello", fileContent);
    }

    [TestMethod]
    public async Task ReadStringAsyncTest()
    {
        string filePath = CreateFile("hello", Encoding.Unicode);
        string fileContent = await BufferedFileOperations.ReadStringAsync(filePath, Encoding.Unicode, 10);
        Assert.AreEqual("hello", fileContent);

        filePath = CreateFile("hello", Encoding.UTF8);
        fileContent = await BufferedFileOperations.ReadStringAsync(filePath, Encoding.UTF8, 10);
        Assert.AreEqual("hello", fileContent);
    }

    [TestMethod]
    public void WriteBytesTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        BufferedFileOperations.WriteBytes(filePath, "hello"u8);
        ReadOnlySpan<byte> fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        BufferedFileOperations.WriteBytes(filePath, "hello"u8);
        fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public async Task WriteBytesAsyncTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        await BufferedFileOperations.WriteBytesAsync(filePath, "hello"u8.ToArray());
        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await BufferedFileOperations.WriteBytesAsync(filePath, "hello"u8.ToArray());
        fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hello"u8));
    }

    [TestMethod]
    public void WriteCharsTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        BufferedFileOperations.WriteChars(filePath, "hello", Encoding.UTF8);
        ReadOnlySpan<char> fileContent = File.ReadAllText(filePath);
        Assert.IsTrue(fileContent is "hello");

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        BufferedFileOperations.WriteChars(filePath, "hello", Encoding.UTF8);
        fileContent = File.ReadAllText(filePath);
        Assert.IsTrue(fileContent is "hello");
    }

    [TestMethod]
    public async Task WriteCharsAsyncTest()
    {
        string filePath = CreateFile("idahwiudhasiudhakwdukawuidha", Encoding.UTF8);
        await BufferedFileOperations.WriteCharsAsync(filePath, "hello".AsMemory(), Encoding.UTF8);
        string fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await BufferedFileOperations.WriteCharsAsync(filePath, "hello".AsMemory(), Encoding.UTF8);
        fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hello", fileContent);
    }

    [TestMethod]
    public void AppendBytesTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        BufferedFileOperations.AppendBytes(filePath, "hello"u8);
        ReadOnlySpan<byte> fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hellohello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        BufferedFileOperations.AppendBytes(filePath, "hello"u8);
        fileContent = File.ReadAllBytes(filePath);
        Assert.IsTrue(fileContent.SequenceEqual("hello"u8));
    }

    [TestMethod]
    public async Task AppendBytesAsyncTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        await BufferedFileOperations.AppendBytesAsync(filePath, "hello"u8.ToArray());
        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hellohello"u8));

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await BufferedFileOperations.AppendBytesAsync(filePath, "hello"u8.ToArray());
        fileContent = await File.ReadAllBytesAsync(filePath);
        Assert.IsTrue(fileContent.AsSpan().SequenceEqual("hello"u8));
    }

    [TestMethod]
    public void AppendCharsTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        BufferedFileOperations.AppendChars(filePath, "hello", Encoding.UTF8);
        string fileContent = File.ReadAllText(filePath);
        Assert.AreEqual("hellohello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        BufferedFileOperations.AppendChars(filePath, "hello", Encoding.UTF8);
        fileContent = File.ReadAllText(filePath);
        Assert.AreEqual("hello", fileContent);
    }

    [TestMethod]
    public async Task AppendCharsAsyncTest()
    {
        string filePath = CreateFile("hello", Encoding.UTF8);
        await BufferedFileOperations.AppendCharsAsync(filePath, "hello".AsMemory(), Encoding.UTF8);
        string fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hellohello", fileContent);

        filePath = CreateFile(string.Empty, Encoding.UTF8);
        await BufferedFileOperations.AppendCharsAsync(filePath, "hello".AsMemory(), Encoding.UTF8);
        fileContent = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual("hello", fileContent);
    }
}
