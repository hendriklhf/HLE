using System;
using HLE.Collections;

namespace HLE.Marshals;

public static class ValueStackMarshal<T>
{
    public static Span<T> GetBuffer(ValueStack<T> stack)
    {
        return stack._stack;
    }
}
