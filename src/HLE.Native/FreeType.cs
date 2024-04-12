using System;

namespace HLE.Native;

[Flags]
public enum FreeType
{
    None = 0,
    Decommit = 0x00004000,
    Release = 0x00008000,
    CoalescePlaceholders = 0x00000001,
    PreservePlaceholder = 0x00000002
}
