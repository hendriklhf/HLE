using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public static class DisposeHelpers
{
    public static void DisposeAll<T>(IEnumerable<T> disposables) where T : IDisposable?
    {
        if (disposables.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            DisposeAll(span);
            return;
        }

        foreach (T disposable in disposables)
        {
            disposable?.Dispose();
        }
    }

    public static void DisposeAll<T>(List<T?> disposables) where T : IDisposable?
        => DisposeAll(CollectionsMarshal.AsSpan(disposables));

    public static void DisposeAll<T>(T?[] disposables) where T : IDisposable?
        => DisposeAll(disposables.AsSpan());

    public static void DisposeAll<T>(Span<T?> disposables) where T : IDisposable?
        => DisposeAll((ReadOnlySpan<T?>)disposables);

    public static void DisposeAll<T>(params ReadOnlySpan<T?> disposables) where T : IDisposable?
    {
        for (int i = 0; i < disposables.Length; i++)
        {
            disposables[i]?.Dispose();
        }
    }
}
