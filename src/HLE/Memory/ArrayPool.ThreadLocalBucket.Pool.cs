using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal partial struct ThreadLocalBucket
    {
        [InlineArray(Length)]
        [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
        private struct Pool
        {
#pragma warning disable RCS1169 // make field readonly
            [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
            [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
            [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
            private T[]? _pool;
#pragma warning restore RCS1169

            private const int Length = 32; // 32 is too much. has to be ArrayPool.BucketCapacities.Length, but it's not const
        }
    }
}
