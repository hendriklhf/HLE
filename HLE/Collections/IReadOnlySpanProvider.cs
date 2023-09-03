using System;

namespace HLE.Collections;

public interface IReadOnlySpanProvider<T>
{
    ReadOnlySpan<T> GetReadOnlySpan();
}
