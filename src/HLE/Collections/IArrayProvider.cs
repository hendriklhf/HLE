using System;

namespace HLE.Collections;

public interface IArrayProvider<out T>
{
    T[] ToArray();

    T[] ToArray(int start);

    T[] ToArray(int start, int length);

    T[] ToArray(Range range);
}
