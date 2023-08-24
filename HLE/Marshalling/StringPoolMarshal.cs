using HLE.Strings;

namespace HLE.Marshalling;

public static class StringPoolMarshal
{
    public static string?[] GetBucketStrings(StringPool stringPool, int bucketIndex)
    {
        return stringPool._buckets[bucketIndex]._strings;
    }
}
