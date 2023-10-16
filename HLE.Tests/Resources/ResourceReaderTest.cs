using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using HLE.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Resources;

[TestClass]
public class ResourceReaderTest
{
    [TestMethod]
    public void ReadResourceTest()
    {
        ResourceReader reader = new(Assembly.GetExecutingAssembly());
        List<string> resources = new();
        for (int i = 1; i <= 3; i++)
        {
            bool success = reader.TryRead($"Resources.Resource{i}", out ImmutableArray<byte> resource);
            Assert.IsTrue(success);
            resources.Add(Encoding.UTF8.GetString(resource.AsSpan()));
        }

        Assert.AreEqual("abc\r\n", resources[0]);
        Assert.AreEqual("xd\r\n", resources[1]);
        Assert.AreEqual(":)\r\n", resources[2]);
    }

    [TestMethod]
    public void ReadResourceOnInit()
    {
        ResourceReader reader = new(Assembly.GetExecutingAssembly(), true);
        List<string> resources = new();
        for (int i = 1; i <= 3; i++)
        {
            bool success = reader.TryRead($"Resources.Resource{i}", out ImmutableArray<byte> resource);
            Assert.IsTrue(success);
            resources.Add(Encoding.UTF8.GetString(resource.AsSpan()));
        }

        Assert.AreEqual("abc\r\n", resources[0]);
        Assert.AreEqual("xd\r\n", resources[1]);
        Assert.AreEqual(":)\r\n", resources[2]);
    }
}
