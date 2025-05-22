#if NET9_0_OR_GREATER
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HLE.Marshalling;

public static partial class ListMarshal
{
    private static class UnsafeAccessor<T>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        public static extern ref T[] GetItems(List<T> list);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_size")]
        public static extern ref int GetSize(List<T> list);
    }
}
#endif
