using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal partial struct ThreadLocalPool
    {
        [InlineArray(Length)]
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
        [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "<Pending>")]
        [SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "<Pending>")]
        [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "<Pending>")]
        [SuppressMessage("Roslynator", "RCS1169:Make field read-only", Justification = "<Pending>")]
        private struct Pool
        {
            private Bucket _buckets;

            // 32 is too much. has to be
            // "BitOperations.TrailingZeroCount(ArrayPoolSettings.MaximumArrayLength) - BitOperations.TrailingZeroCount(ArrayPoolSettings.MinimumArrayLength) + 1",
            // but it's not const
            public const int Length = 32;

            public struct Bucket
            {
                public ArrayBuffer Arrays;
                public uint Count;

                [InlineArray(ThreadLocalArraysPerLength)]
                public struct ArrayBuffer
                {
                    private T[]? _arrays;
                }
            }
        }
    }
}
