using HLE.Strings;

namespace HLE.Marshals;

public static class StringArrayMarshal
{
    public static string[] GetStrings(StringArray array)
    {
        return array._strings;
    }

    public static int[] GetStringLengths(StringArray array)
    {
        return array._stringLengths;
    }

    public static int[] GetStringStartIndices(StringArray array)
    {
        return array._stringStarts;
    }

    public static char[] GetCharBuffer(StringArray array)
    {
        return array._stringChars;
    }
}
