using System.Collections.Generic;
using System.Text;
using HLE.Resources;

namespace HLE.UnitTests.Resources;

public sealed class ResourceReaderTest
{
    [Fact]
    public void ReadResourceTest()
    {
        using ResourceReader reader = new(typeof(ResourceReaderTest).Assembly);
        List<string> resources = [];
        for (int i = 1; i <= 3; i++)
        {
            bool success = reader.TryRead($"{typeof(ResourceReaderTest).Namespace}.Resource{i}", out Resource resource);
            Assert.True(success);
            resources.Add(Encoding.UTF8.GetString(resource.AsSpan()));
        }

        Assert.Equal("abc\r\n", resources[0]);
        Assert.Equal("xd\r\n", resources[1]);
        Assert.Equal(":)\r\n", resources[2]);
    }

    [Fact]
    public void TryRead_ReturnsFalse()
    {
        using ResourceReader reader = new(typeof(ResourceReaderTest).Assembly);
        bool success = reader.TryRead("wdauhwdiuahwd", out Resource resource);
        Assert.False(success);
        Assert.Equal(Resource.Empty, resource);
    }

    [Fact]
    public void ReadResource_LogicalName_Test()
    {
        using ResourceReader reader = new(typeof(ResourceReaderTest).Assembly);
        Resource resource = reader.Read("R4");
        Assert.Equal("hello\r\n", Encoding.UTF8.GetString(resource.AsSpan()));
    }
}
