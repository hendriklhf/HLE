using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace HLE.Tests;

public sealed partial class RandomExtensionsTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void BothRandomExtensionsHaveTheSameMethods()
    {
        ReadOnlySpan<RandomExtensionMethod> randomMethods = typeof(RandomExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static m => new RandomExtensionMethod(m))
            .ToArray();

        ReadOnlySpan<RandomExtensionMethod> randomNumberGeneratorMethods = typeof(RandomNumberGeneratorExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static m => new RandomExtensionMethod(m))
            .ToArray();

        int missingRandomExtensionsMethodCount = 0;
        for (int i = 0; i < randomMethods.Length; i++)
        {
            RandomExtensionMethod method = randomMethods[i];
            if (!randomNumberGeneratorMethods.Contains(method))
            {
                missingRandomExtensionsMethodCount++;
                _testOutputHelper.WriteLine($"{typeof(RandomNumberGeneratorExtensions)} does not contain: {method}");
            }
        }

        int missingRandomNumberGeneratorExtensionsMethodCount = 0;
        for (int i = 0; i < randomNumberGeneratorMethods.Length; i++)
        {
            RandomExtensionMethod method = randomNumberGeneratorMethods[i];
            if (!randomMethods.Contains(method))
            {
                missingRandomNumberGeneratorExtensionsMethodCount++;
                _testOutputHelper.WriteLine($"{typeof(RandomExtensions)} does not contain: {method}");
            }
        }

        Assert.Equal(0, missingRandomExtensionsMethodCount);
        Assert.Equal(0, missingRandomNumberGeneratorExtensionsMethodCount);
    }
}
