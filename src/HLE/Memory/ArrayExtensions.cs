using System.Collections.Generic;

namespace HLE.Memory;

public static class ArrayExtensions
{
    public static void CopyTo<T>(this T[] source, List<T> destination)
    {
        CopyWorker<T> copyWorker = new(source);
        copyWorker.CopyTo(destination);
    }

    public static void CopyTo<T>(this T[] source, T[] destination) => SpanHelpers<T>.Copy(source, destination);

    // already exist:
    // public static void CopyTo<T>(this T[] source, Memory<T> destination)
    // public static void CopyTo<T>(this T[] source, Span<T> destination)

    public static void CopyTo<T>(this T[] source, ref T destination)
        => SpanHelpers<T>.Copy(source, ref destination);

    public static unsafe void CopyTo<T>(this T[] source, T* destination)
        => SpanHelpers<T>.Copy(source, destination);
}
