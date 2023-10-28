using System;

namespace HLE.Memory;

[Flags]
public enum ArrayReturnOptions
{
    None,
    Clear = 1,
    ClearOnlyIfManagedType = 2
}
