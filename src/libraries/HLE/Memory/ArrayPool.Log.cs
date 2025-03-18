using System.Diagnostics;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal static unsafe class Log
    {
        [Conditional("DEBUG")]
        public static void Rented(T[] array)
            => Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Rented {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");

        [Conditional("DEBUG")]
        public static void Allocated(T[] array)
            => Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Allocated {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");

        [Conditional("DEBUG")]
        public static void Returned(T[] array)
            => Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Returned {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");

        [Conditional("DEBUG")]
        public static void Dropped(T[] array)
            => Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Dropped {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");
    }
}
