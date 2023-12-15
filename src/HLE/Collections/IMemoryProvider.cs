using System;

namespace HLE.Collections;

public interface IMemoryProvider<T> : IReadOnlyMemoryProvider<T>
{
    Memory<T> GetMemory();
}
