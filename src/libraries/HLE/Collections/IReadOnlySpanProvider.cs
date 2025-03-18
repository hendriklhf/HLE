using System;

namespace HLE.Collections;

public interface IReadOnlySpanProvider<T>
{
    ReadOnlySpan<T> AsSpan();

    ReadOnlySpan<T> AsSpan(int start);

    ReadOnlySpan<T> AsSpan(int start, int length);

    ReadOnlySpan<T> AsSpan(Range range);
}
