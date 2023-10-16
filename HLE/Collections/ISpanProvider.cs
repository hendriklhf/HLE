using System;

namespace HLE.Collections;

public interface ISpanProvider<T> : IReadOnlySpanProvider<T>
{
    /// <summary>
    /// Gets a span of objects associated with the collection.
    /// </summary>
    Span<T> GetSpan();

#pragma warning disable CA1033
    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => GetSpan();
#pragma warning restore CA1033
}
