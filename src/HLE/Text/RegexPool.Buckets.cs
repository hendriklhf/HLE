using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Text;

public sealed partial class RegexPool
{
    [InlineArray(Length)]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
    private struct Buckets
    {
        private Bucket _bucket;

        public const int Length = DefaultPoolCapacity;
    }
}
