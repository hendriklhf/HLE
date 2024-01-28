using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static class ListMarshal
{
    [Pure]
    public static Memory<T> AsMemory<T>(List<T> list) => SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(list));

    [Pure]
    public static T[] AsArray<T>(List<T> list) => SpanMarshal.AsArray(CollectionsMarshal.AsSpan(list));
}
