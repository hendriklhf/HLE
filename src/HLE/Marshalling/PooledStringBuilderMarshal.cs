using HLE.Strings;

namespace HLE.Marshalling;

public static class PooledStringBuilderMarshal
{
    public static char[] GetBuffer(PooledStringBuilder builder) => builder.GetBuffer();
}
