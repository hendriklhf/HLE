using HLE.Strings;

namespace HLE.Marshals;

public static class PooledStringBuilderMarshal
{
    public static char[] GetBuffer(PooledStringBuilder builder)
    {
        return builder._buffer._array;
    }
}
