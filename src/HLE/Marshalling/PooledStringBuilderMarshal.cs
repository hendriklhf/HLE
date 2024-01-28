using HLE.Strings;

namespace HLE.Marshalling;

public static class PooledStringBuilderMarshal
{
    public static char[] GetBuffer(PooledStringBuilder builder)
    {
        char[]? buffer = builder._buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<PooledStringBuilder>();
        }

        return buffer;
    }
}
