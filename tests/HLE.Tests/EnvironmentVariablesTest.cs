using System;
using System.Collections;
using Xunit;

namespace HLE.Tests;

public sealed class EnvironmentVariablesTest
{
    [Fact]
    public void CreateTest()
    {
        EnvironmentVariables environmentVariables = EnvironmentVariables.Create();
        IDictionary actualEnvironmentVariables = Environment.GetEnvironmentVariables();
        Assert.Equal(actualEnvironmentVariables.Count, environmentVariables.Count);

        foreach (object? obj in actualEnvironmentVariables)
        {
            DictionaryEntry entry = (DictionaryEntry)obj;
            string? value = environmentVariables[(string)entry.Key];
            Assert.NotNull(value);
            Assert.NotNull(entry.Value);
            Assert.Equal(entry.Value, value);
        }
    }
}
