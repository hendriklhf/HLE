using System;

namespace HLE.Memory;

public unsafe interface ICopyable<T>
{
    void CopyTo(T[] destination);

    void CopyTo(Memory<T> destination);

    void CopyTo(Span<T> destination);

    void CopyTo(ref T destination);

    void CopyTo(T* destination);
}
