namespace HLE.Collections;

public static class EmptyEnumeratorCache<T>
{
    public static EmptyEnumerator<T> Enumerator { get; } = new();
}
