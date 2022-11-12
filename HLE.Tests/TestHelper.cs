using System.Linq;

namespace HLE.Tests;

public static class TestHelper
{
    public static int[] CreateIntArray(int length)
    {
        return Enumerable.Range(0, length).Select(_ => Random.Int()).ToArray();
    }

    public static string[] CreateStringArray(int length, int stringLength, ushort minChar = ushort.MinValue, ushort maxChar = ushort.MaxValue)
    {
        return Enumerable.Range(0, length).Select(_ => Random.String(stringLength, minChar, maxChar)).ToArray();
    }
}
