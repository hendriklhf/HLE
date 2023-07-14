using HLE.Strings;

namespace HLE.Marshals;

public static class StringPoolMarshal
{
    public static string?[] GetBucketStrings(StringPool stringPool, int bucketIndex)
    {
        return stringPool._buckets[bucketIndex]._strings;
    }
}
