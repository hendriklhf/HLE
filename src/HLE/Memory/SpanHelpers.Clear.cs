using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    public static unsafe void Clear<T>(T* items, nuint count) => Clear(ref Unsafe.AsRef<T>(items), count);

    public static void Clear<T>(ref T items, nuint count)
    {
        if (count <= int.MaxValue)
        {
            MemoryMarshal.CreateSpan(ref items, (int)count).Clear();
            return;
        }

        ClearCore(ref items, count);

        return;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ClearCore(ref T items, nuint count)
        {
            Debug.Assert(count > int.MaxValue);

            do
            {
                MemoryMarshal.CreateSpan(ref items, int.MaxValue).Clear();
                items = ref Unsafe.Add(ref items, int.MaxValue);
                count -= int.MaxValue;
            }
            while (count >= int.MaxValue);

            if (count != 0)
            {
                MemoryMarshal.CreateSpan(ref items, (int)count).Clear();
            }
        }
    }
}
