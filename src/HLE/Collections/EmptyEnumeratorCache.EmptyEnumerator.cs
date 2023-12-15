using System.Collections;
using System.Collections.Generic;

namespace HLE.Collections;

public static partial class EmptyEnumeratorCache<T>
{
    private sealed class EmptyEnumerator : IEnumerator<T>
    {
        public T Current => default!;

        object IEnumerator.Current => Current!;

        public bool MoveNext() => false;

        public void Reset()
        {
        }

        public void Dispose()
        {
        }
    }
}
