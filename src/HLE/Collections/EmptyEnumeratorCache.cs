using System.Collections.Generic;

namespace HLE.Collections;

public static partial class EmptyEnumeratorCache<T>
{
    public static IEnumerator<T> Enumerator { get; } = new EmptyEnumerator();
}
