using System.Buffers;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static class ArrayPoolExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReturnAndClearIfManaged<T>(this ArrayPool<T> pool, T[] array, int count)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SpanHelpers.Clear(array, count);
        }

        pool.Return(array);
    }
}
