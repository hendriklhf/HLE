using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HLE.Marshalling;
using HLE.Text;
#if DEBUG
using System.Threading;
#endif

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal static unsafe class Log
    {
#if DEBUG
        public static ulong SharedRentCounter => s_sharedRentCounter;

        public static ulong ThreadLocalRentCounter => s_threadLocalRentCounter;

        public static ulong AllocationCounter => s_allocationCounter;

        public static ulong SharedReturnCounter => s_sharedReturnCounter;

        public static ulong ThreadLocalReturnCounter => s_threadLocalReturnCounter;

        public static ulong DropCounter => s_dropCounter;

        [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
        private static ulong s_sharedRentCounter;

        [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
        private static ulong s_threadLocalRentCounter;

        [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
        private static ulong s_allocationCounter;

        [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
        private static ulong s_sharedReturnCounter;

        [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
        private static ulong s_threadLocalReturnCounter;

        [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
        private static ulong s_dropCounter;
#endif

        [Conditional("DEBUG")]
        public static void RentedShared(T[] array)
        {
#if DEBUG
            Interlocked.Increment(ref s_sharedRentCounter);
#endif
            Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Rented (Shared) {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");
        }

        [Conditional("DEBUG")]
        public static void RentedThreadLocal(T[] array)
        {
#if DEBUG
            Interlocked.Increment(ref s_threadLocalRentCounter);
#endif
            Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Rented (ThreadLocal) {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");
        }

        [Conditional("DEBUG")]
        public static void Allocated(T[] array)
        {
#if DEBUG
            Interlocked.Increment(ref s_allocationCounter);
#endif
            Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Allocated {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");
        }

        [Conditional("DEBUG")]
        public static void ReturnedShared(T[] array)
        {
#if DEBUG
            Interlocked.Increment(ref s_sharedReturnCounter);
#endif
            Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Returned (Shared) {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");
        }

        [Conditional("DEBUG")]
        public static void ReturnedThreadLocal(T[] array)
        {
#if DEBUG
            Interlocked.Increment(ref s_threadLocalReturnCounter);
#endif
            Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Returned (ThreadLocal) {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");
        }

        [Conditional("DEBUG")]
        public static void Dropped(T[] array)
        {
#if DEBUG
            Interlocked.Increment(ref s_dropCounter);
#endif
            Debug.WriteLine($"{TypeFormatter.Default.Format<ArrayPool<T>>()}: Dropped {TypeFormatter.Default.Format<T>()}[{array.Length}] (0x{(nuint)ObjectMarshal.GetMethodTablePointer(array):X})");
        }
    }
}
