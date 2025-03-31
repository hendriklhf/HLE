using System.Collections.Generic;

namespace HLE.Memory;

public static class ArrayExtensions
{
    public static void CopyTo<T>(this T[] source, List<T> destination, int offset = 0)
        => SpanHelpers.CopyChecked(source, destination, offset);

    public static void CopyTo<T>(this T[] source, ref T destination)
        => SpanHelpers.Copy(source, ref destination);

    public static unsafe void CopyTo<T>(this T[] source, T* destination)
        => SpanHelpers.Copy(source, destination);
}
