using System;

namespace HLE.Marshalling.Windows;

[Flags]
public enum AllocationType
{
    None = 0,
    Commit = 0x00001000,
    Reserve = 0x00002000,
    Reset = 0x00080000,
    TopDown = 0x00100000,
    WriteWatch = 0x00200000,
    Physical = 0x00400000,
    ResetUndo = 0x1000000,
    LargePages = 0x20000000
}
