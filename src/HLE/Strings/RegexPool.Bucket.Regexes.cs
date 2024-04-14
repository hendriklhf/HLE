using System;
using System.Diagnostics.CodeAnalysis;
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
        private struct Regexes
        {
            private Regex? _regexes;

            public ref Regex? Reference => ref Unsafe.AsRef(ref _regexes);

            public Span<Regex?> AsSpan() => MemoryMarshal.CreateSpan(ref _regexes, DefaultBucketCapacity);
        }
    }
}
