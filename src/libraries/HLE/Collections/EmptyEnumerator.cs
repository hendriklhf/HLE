using System.Collections;
using System.Collections.Generic;

namespace HLE.Collections;

public sealed class EmptyEnumerator<T> : IEnumerator<T>
{
    public T Current => default!;

    object? IEnumerator.Current => null;

    public bool MoveNext() => false;

    public void Reset()
    {
    }

    public void Dispose()
    {
    }
}
