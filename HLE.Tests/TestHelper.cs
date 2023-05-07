using System;
using System.Linq;

namespace HLE.Tests;

public static class TestHelper
{
    public static int[] CreateIntArray(int length)
    {
        return Enumerable.Range(0, length).Select(_ => Random.Shared.Next()).ToArray();
    }

    public static string[] CreateStringArray(int arrayLength, int stringLength, char minChar = (char)ushort.MinValue, char maxChar = (char)ushort.MaxValue)
    {
        return Enumerable.Range(0, arrayLength).Select(_ => Random.Shared.NextString(stringLength, minChar, maxChar)).ToArray();
    }
}
