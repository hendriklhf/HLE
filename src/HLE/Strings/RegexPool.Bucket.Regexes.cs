using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace HLE.Strings;

public sealed partial class RegexPool
{
    private partial struct Bucket
    {
        [InlineArray(DefaultBucketCapacity)]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
        [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
        private struct Regexes
        {
            private Regex? _regexes;

            public ref Regex? Reference => ref Unsafe.AsRef(ref _regexes);

            [Pure]
            public Span<Regex?> AsSpan() => MemoryMarshal.CreateSpan(ref _regexes, DefaultBucketCapacity);
        }
    }
}
