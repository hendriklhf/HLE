using System;

namespace HLE.Collections;

public interface ISpanProvider<T> : IReadOnlySpanProvider<T>
{
    /// <summary>
    /// Gets a span of objects associated with the collection.
    /// </summary>
    Span<T> GetSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => GetSpan();
}
