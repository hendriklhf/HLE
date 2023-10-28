using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HLE.Resources;
using Xunit;

namespace HLE.Tests.Resources;

public sealed class ResourceReaderTest
{
    [Fact]
    public void ReadResourceTest()
    {
        ResourceReader reader = new(Assembly.GetExecutingAssembly());
        List<string> resources = [];
        for (int i = 1; i <= 3; i++)
        {
            bool success = reader.TryRead($"Resources.Resource{i}", out Resource resource);
            Assert.True(success);
            resources.Add(Encoding.UTF8.GetString(resource.AsSpan()));
        }

        Assert.Equal("abc\r\n", resources[0]);
        Assert.Equal("xd\r\n", resources[1]);
        Assert.Equal(":)\r\n", resources[2]);
    }
}
