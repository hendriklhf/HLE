using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace HLE.Strings;

public sealed partial class RegexPool
{
    private partial struct Bucket
    {
        [InlineArray(Length)]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
        [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
        [SuppressMessage("Roslynator", "RCS1169:Make field read-only")]
        [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
        [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
        private struct Regexes
        {
            private Regex? _regexes;

            public const int Length = DefaultBucketCapacity;
        }
    }
}
