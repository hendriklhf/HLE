using System.Collections.Generic;
using System.Linq;
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
        ResourceReader reader = new(Assembly.GetExecutingAssembly(), false);
        List<string?> resources = new();
        for (int i = 1; i <= 3; i++)
        {
            byte[]? resource = reader.Read($"Resources.Resource{i}");
            resources.Add(Encoding.UTF8.GetString(resource!));
        }

        Assert.IsTrue(resources.Count(r => r is not null) == 3);
        Assert.AreEqual("abc\r\n", resources[0]);
        Assert.AreEqual("xd\r\n", resources[1]);
        Assert.AreEqual(":)\r\n", resources[2]);
    }

    [TestMethod]
    public void ReadResourceOnInit()
    {
        ResourceReader reader = new(Assembly.GetExecutingAssembly());
        List<string?> resources = new();
        for (int i = 1; i <= 3; i++)
        {
            byte[]? resource = reader.Read($"Resources.Resource{i}");
            resources.Add(Encoding.UTF8.GetString(resource!));
        }

        Assert.IsTrue(resources.Count(r => r is not null) == 3);
        Assert.AreEqual("abc\r\n", resources[0]);
        Assert.AreEqual("xd\r\n", resources[1]);
        Assert.AreEqual(":)\r\n", resources[2]);
    }
}
