using System.Reflection;
using Xunit;

namespace HLE.Tests;

public sealed class RandomExtensionsTest
{
    [Fact]
    public void BothRandomExtensionsHaveTheSameMethods()
    {
        MethodInfo[] randomMethods = typeof(RandomExtensions).GetMethods();
        MethodInfo[] randomNumberGeneratorMethods = typeof(RandomNumberGeneratorExtensions).GetMethods();

        Assert.Equal(randomMethods.Length, randomNumberGeneratorMethods.Length);
    }
}
