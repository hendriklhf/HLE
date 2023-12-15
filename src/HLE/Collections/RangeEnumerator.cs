using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public ref struct RangeEnumerator
{
    public int Current { get; private set; }

    private readonly int _end;

    public RangeEnumerator(Range range)
    {
        if (range.End.IsFromEnd)
        {
            ThrowRangeEndStartsFromEnd();
        }

        Current = range.Start.Value - 1;
        _end = range.End.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() => ++Current <= _end;

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowRangeEndStartsFromEnd()
        => throw new InvalidOperationException($"Can't enumerate a {typeof(Range)} whose end starts from the end.");
}
