using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public struct RangeEnumerator : IEnumerator<int>, IEquatable<RangeEnumerator>
{
    public int Current { get; private set; }

    readonly object IEnumerator.Current => Current;

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

    public bool MoveNext() => ++Current <= _end;

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowRangeEndStartsFromEnd()
        => throw new InvalidOperationException($"Can't enumerate a {typeof(Range)} whose end starts from the end.");

    [DoesNotReturn]
    public readonly void Reset() => throw new NotSupportedException();

    public readonly void Dispose()
    {
    }

    public readonly bool Equals(RangeEnumerator other) => _end == other._end && Current == other.Current;

    public override readonly bool Equals(object? obj) => obj is RangeEnumerator other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_end, Current);

    public static bool operator ==(RangeEnumerator left, RangeEnumerator right) => left.Equals(right);

    public static bool operator !=(RangeEnumerator left, RangeEnumerator right) => !left.Equals(right);
}
