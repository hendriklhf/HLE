using System;
using System.Collections.Generic;

namespace HLE.Collections;

public static class DisposeHelpers<T> where T : IDisposable
{
    public static void DisposeAll(IEnumerable<T> disposables)
    {
        if (disposables.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            DisposeAll(span);
            return;
        }

        foreach (T disposable in disposables)
        {
            disposable.Dispose();
        }
    }

    public static void DisposeAll(T[] disposables) => DisposeAll(disposables.AsSpan());

    public static void DisposeAll(Span<T> disposables) => DisposeAll((ReadOnlySpan<T>)disposables);

    public static void DisposeAll(ReadOnlySpan<T> disposables)
    {
        for (int i = 0; i < disposables.Length; i++)
        {
            disposables[i].Dispose();
        }
    }
}
