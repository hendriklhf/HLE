using System;

namespace HLE;

[Flags]
internal enum RandomWritingOptimizations
{
    None = 0,
    ChoicesLengthIsPow2 = 1,
    ChoicesLengthIs8Bits = 2,
    ChoicesLengthIs16Bits = 4,
    All = ChoicesLengthIsPow2 | ChoicesLengthIs8Bits | ChoicesLengthIs16Bits
}
