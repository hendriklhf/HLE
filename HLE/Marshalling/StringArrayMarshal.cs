using HLE.Strings;

namespace HLE.Marshalling;

public static class StringArrayMarshal
{
    public static string[] GetStrings(StringArray array) => array._strings;

    public static int[] GetStringLengths(StringArray array) => array._stringLengths;

    public static int[] GetStringStartIndices(StringArray array) => array._stringStarts;

    public static char[] GetCharBuffer(StringArray array) => array._stringChars;
}
