using System;
using HLE.Strings;

namespace HLE.Marshalling;

public static class ValueStringBuilderMarshal
{
    public static Span<char> GetBuffer(ValueStringBuilder builder)
    {
        return builder._buffer;
    }
}
