using System;
using System.Collections.Generic;

namespace HLE.Collections;

public interface ICollectionProvider<T>
{
    T[] ToArray();

    T[] ToArray(int start);

    T[] ToArray(int start, int length);

    T[] ToArray(Range range);

    List<T> ToList();
}
