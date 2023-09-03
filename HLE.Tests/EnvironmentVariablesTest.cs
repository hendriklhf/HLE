using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class EnvironmentVariablesTest
{
    [TestMethod]
    public void CreateTest()
    {
        EnvironmentVariables environmentVariables = EnvironmentVariables.Create();
        IDictionary actualEnvironmentVariables = Environment.GetEnvironmentVariables();
        Assert.AreEqual(actualEnvironmentVariables.Count, environmentVariables.Count);

        foreach (object? obj in actualEnvironmentVariables)
        {
            DictionaryEntry entry = (DictionaryEntry)obj;
            string? value = environmentVariables[(string)entry.Key];
            Assert.IsNotNull(value);
            Assert.IsNotNull(entry.Value);
            Assert.AreEqual(entry.Value, value);
        }
    }
}
