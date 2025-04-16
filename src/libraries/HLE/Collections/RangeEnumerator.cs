using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HLE.Collections;

public struct RangeEnumerator : IEnumerator<int>, IEquatable<RangeEnumerator>
{
    public readonly int Current => _current;

    readonly object IEnumerator.Current => Current;

    private int _current;
    private readonly int _end;

    public static RangeEnumerator Empty => default;

    public RangeEnumerator(Range range)
    {
        if (range.End.IsFromEnd)
        {
            ThrowRangeEndStartsFromEnd();
        }

        _current = range.Start.Value - 1;
        _end = range.End.Value;

        return;

        [DoesNotReturn]
        static void ThrowRangeEndStartsFromEnd()
            => throw new InvalidOperationException($"Can't enumerate a {typeof(Range)} whose end starts from the end.");
    }

    public bool MoveNext() => ++_current <= _end;

    [DoesNotReturn]
    readonly void IEnumerator.Reset() => throw new NotSupportedException();

    readonly void IDisposable.Dispose()
    {
    }

    public readonly bool Equals(RangeEnumerator other) => _end == other._end && Current == other.Current;

    public override readonly bool Equals(object? obj) => obj is RangeEnumerator other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_end, Current);

    public static bool operator ==(RangeEnumerator left, RangeEnumerator right) => left.Equals(right);

    public static bool operator !=(RangeEnumerator left, RangeEnumerator right) => !left.Equals(right);
}
