using System;

namespace HLE.Collections;

public ref struct RangeEnumerator
{
    public int Current { get; private set; }

    private readonly int _end;

    public RangeEnumerator(Range range)
    {
        if (range.End.IsFromEnd)
        {
            throw new InvalidOperationException($"Can't enumerate a {typeof(Range)} whose end starts from the end.");
        }

        Current = range.Start.Value - 1;
        _end = range.End.Value;
    }

    public bool MoveNext()
    {
        Current++;
        return Current <= _end;
    }
}
