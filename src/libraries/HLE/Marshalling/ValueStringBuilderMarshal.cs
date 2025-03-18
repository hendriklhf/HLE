using System;
using HLE.Text;

namespace HLE.Marshalling;

public static class ValueStringBuilderMarshal
{
    public static Span<char> GetBuffer(ValueStringBuilder builder) => builder.GetBuffer();
}
