using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.Collections;

public static class DisposeHelpers
{
    public static void DisposeAll<T>(IEnumerable<T> disposables) where T : IDisposable
    {
        if (disposables.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            DisposeAll(span);
            return;
        }

        foreach (T disposable in disposables)
        {
            disposable.Dispose();
        }
    }

    public static void DisposeAll<T>(List<T> disposables) where T : IDisposable
        => DisposeAll(ref ListMarshal.GetReference(disposables), disposables.Count);

    public static void DisposeAll<T>(T[] disposables) where T : IDisposable
        => DisposeAll(ref MemoryMarshal.GetArrayDataReference(disposables), disposables.Length);

    public static void DisposeAll<T>(Span<T> disposables) where T : IDisposable
        => DisposeAll(ref MemoryMarshal.GetReference(disposables), disposables.Length);

    public static void DisposeAll<T>(params ReadOnlySpan<T> disposables) where T : IDisposable
        => DisposeAll(ref MemoryMarshal.GetReference(disposables), disposables.Length);

    private static void DisposeAll<T>(ref T reference, int length) where T : IDisposable
    {
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref reference, i).Dispose();
        }
    }
}
