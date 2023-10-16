using System;
using HLE.Collections;

namespace HLE.Marshalling;

public static class ValueStackMarshal<T>
{
    public static Span<T> GetBuffer(ValueStack<T> stack) => stack._stack;
}
