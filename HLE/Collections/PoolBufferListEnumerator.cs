using System;
using System.Collections;
using System.Collections.Generic;

namespace HLE.Collections;

public struct PoolBufferListEnumerator<T> : IEnumerator<T> where T : IEquatable<T>
{
    public T Current { get; private set; }

    readonly object? IEnumerator.Current => Current;

    private PoolBufferList<T> _poolBufferList;
    private int _currentIndex;

    public PoolBufferListEnumerator(PoolBufferList<T> poolBufferList)
    {
        _poolBufferList = poolBufferList;
        Current = default!;
    }

    public bool MoveNext()
    {
        if (_poolBufferList.Count == 0)
        {
            return false;
        }

        bool success = _currentIndex < _poolBufferList.Count;
        if (!success)
        {
            return false;
        }

        Current = _poolBufferList[_currentIndex++];
        return success;
    }

    public void Reset()
    {
        _currentIndex = 0;
    }

    public void Dispose()
    {
        Current = default!;
        _poolBufferList = null!;
    }
}
