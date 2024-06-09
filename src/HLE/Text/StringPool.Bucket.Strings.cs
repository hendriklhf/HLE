using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Text;

public sealed partial class StringPool
{
    private partial struct Bucket
    {
        [InlineArray(Length)]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
        [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
        private struct Strings
        {
#pragma warning disable RCS1169
            [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
            [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
            [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
            private string? _strings;
#pragma warning restore RCS1169

            public const int Length = DefaultBucketCapacity;
        }
    }
}
