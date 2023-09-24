using System;

namespace HLE.Collections;

public interface IReadOnlySpanProvider<T>
{
    /// <summary>
    /// Gets a span of objects associated with the collection.
    /// </summary>
    ReadOnlySpan<T> GetReadOnlySpan();
}
