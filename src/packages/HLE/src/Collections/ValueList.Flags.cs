using System;

namespace HLE.Collections;

public ref partial struct ValueList<T>
{
    [Flags]
    private enum Flags
    {
        None = 0,
        IsDisposed = 1 << 0,
        IsRentedArray = 1 << 1
    }
}
