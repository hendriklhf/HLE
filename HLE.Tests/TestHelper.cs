using System;
using System.Linq;

namespace HLE.Tests;

public static class TestHelper
{
    public static int[] CreateRandomIntArray(int length)
    {
        return Enumerable.Range(0, length).Select(static _ => Random.Shared.Next()).ToArray();
    }

    public static string[] CreateRandomStringArray(int arrayLength, int stringLength, char minChar = char.MinValue, char maxChar = char.MaxValue)
    {
        return Enumerable.Range(0, arrayLength).Select(_ => Random.Shared.NextString(stringLength, minChar, maxChar)).ToArray();
    }
}
