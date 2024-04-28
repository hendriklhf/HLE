using System;

namespace HLE.Marshalling.Windows;

[Flags]
public enum FreeTypes
{
    None = 0,
    CoalescePlaceholders = 0x00000001,
    PreservePlaceholder = 0x00000002,
    Decommit = 0x00004000,
    Release = 0x00008000
}
