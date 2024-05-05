using System;
using System.Collections.Generic;

namespace HLE.Collections;

public interface IListProvider<T>
{
    List<T> ToList();

    List<T> ToList(int start);

    List<T> ToList(int start, int length);

    List<T> ToList(Range range);
}
