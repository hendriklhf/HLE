using System;

namespace HLE.Text;

public ref partial struct ValueStringBuilder
{
    [Flags]
    private enum Flags
    {
        None = 0,
        IsDisposed = 1 << 0,
        IsRentedArray = 1 << 1
    }
}
