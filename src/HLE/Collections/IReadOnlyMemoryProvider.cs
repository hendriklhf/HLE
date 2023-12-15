using System;

namespace HLE.Collections;

public interface IReadOnlyMemoryProvider<T>
{
    ReadOnlyMemory<T> GetReadOnlyMemory();
}
