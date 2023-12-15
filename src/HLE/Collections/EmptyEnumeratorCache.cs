using System.Collections.Generic;

namespace HLE.Collections;

public static partial class EmptyEnumeratorCache<T>
{
    private static readonly EmptyEnumerator s_enumerator = new();

    public static IEnumerator<T> Enumerator => s_enumerator;
}
