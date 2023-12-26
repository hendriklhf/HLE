using System;
using HLE.Collections;

namespace HLE.Marshalling;

public static class ValueStackMarshal
{
    public static Span<T> GetBuffer<T>(ValueStack<T> stack) => stack._stack;
}
