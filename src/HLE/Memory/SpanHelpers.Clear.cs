using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    public static unsafe void Clear<T>(T* items, nuint elementCount) => Clear(ref Unsafe.AsRef<T>(items), elementCount);

    public static void Clear<T>(ref T items, nuint elementCount)
    {
        if (elementCount <= int.MaxValue)
        {
            MemoryMarshal.CreateSpan(ref items, (int)elementCount).Clear();
            return;
        }

        ClearCore(ref items, elementCount);

        return;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ClearCore(ref T items, nuint elementCount)
        {
            Debug.Assert(elementCount > int.MaxValue);

            do
            {
                MemoryMarshal.CreateSpan(ref items, int.MaxValue).Clear();
                items = ref Unsafe.Add(ref items, int.MaxValue);
                elementCount -= int.MaxValue;
            }
            while (elementCount >= int.MaxValue);

            if (elementCount != 0)
            {
                MemoryMarshal.CreateSpan(ref items, (int)elementCount).Clear();
            }
        }
    }
}
