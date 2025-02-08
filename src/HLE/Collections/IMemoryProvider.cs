using System;

namespace HLE.Collections;

public interface IMemoryProvider<T>
{
    Memory<T> AsMemory();

    Memory<T> AsMemory(int start);

    Memory<T> AsMemory(int start, int length);

    Memory<T> AsMemory(Range range);
}
