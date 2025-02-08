using System;

namespace HLE.Collections;

public interface ISpanProvider<T>
{
    Span<T> AsSpan();

    Span<T> AsSpan(int start);

    Span<T> AsSpan(int start, int length);

    Span<T> AsSpan(Range range);
}
