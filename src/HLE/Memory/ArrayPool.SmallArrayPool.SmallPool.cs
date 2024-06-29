using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    private partial struct SmallArrayPool
    {
        [InlineArray(ArrayPool.MinimumArrayLength - 2)]
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
        private partial struct SmallPool
        {
#pragma warning disable IDE0051
            private SmallBucket _buckets;
#pragma warning restore IDE0051
        }
    }
}
