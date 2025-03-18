using System;
using System.Collections.Generic;

namespace HLE.Collections;

public unsafe interface ICopyable<T> : ICountable
{
    void CopyTo(List<T> destination, int offset = 0);

    void CopyTo(T[] destination, int offset = 0);

    void CopyTo(Memory<T> destination);

    void CopyTo(Span<T> destination);

    void CopyTo(ref T destination);

    void CopyTo(T* destination);
}
