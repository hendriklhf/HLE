using System;

namespace HLE.Collections;

public interface ISpanProvider<T> : IReadOnlySpanProvider<T>
{
    Span<T> GetSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => GetSpan();
}
