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
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
        [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
        [SuppressMessage("Roslynator", "RCS1169:Make field read-only")]
        [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
        private struct Pool
        {
            private T[]? _pool;

            private const int Length = 32; // 32 is too much. has to be ArrayPool.BucketCapacities.Length, but it's not const
        }
    }
}
