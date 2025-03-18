using System;

namespace HLE.Collections;

public interface IReadOnlyMemoryProvider<T>
{
    ReadOnlyMemory<T> AsMemory();

    ReadOnlyMemory<T> AsMemory(int start);

    ReadOnlyMemory<T> AsMemory(int start, int length);

    ReadOnlyMemory<T> AsMemory(Range range);
}
