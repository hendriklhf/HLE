using System.Collections.Generic;

namespace HLE.Collections;

public interface ICollectionProvider<T>
{
    T[] ToArray();

    List<T> ToList();
}
