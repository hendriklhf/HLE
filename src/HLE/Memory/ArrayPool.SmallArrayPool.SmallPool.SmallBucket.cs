using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    private partial struct SmallArrayPool
    {
        private partial struct SmallPool
        {
            [InlineArray(Length)]
            [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
            [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
            [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
            [SuppressMessage("Roslynator", "RCS1169:Make field read-only")]
            internal struct SmallBucket
            {
                private T[]? _arrays;

                public const int Length = 8;
            }
        }
    }
}
