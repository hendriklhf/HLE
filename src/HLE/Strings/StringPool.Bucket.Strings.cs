using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Strings;

public sealed partial class StringPool
{
    private partial struct Bucket
    {
        [InlineArray(DefaultBucketCapacity)]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
        [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
        private struct Strings
        {
            private string? _strings;

            public ref string? Reference => ref Unsafe.AsRef(ref _strings);

            [Pure]
            public Span<string?> AsSpan() => MemoryMarshal.CreateSpan(ref _strings, DefaultBucketCapacity);
        }
    }
}
