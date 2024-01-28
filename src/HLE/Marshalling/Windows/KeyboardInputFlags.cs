using System;

namespace HLE.Marshalling.Windows;

[Flags]
public enum KeyboardInputFlags
{
    None = 0,
    ExtendedKey = 1,
    KeyUp = 2,
    Unicode = 4,
    ScanCode = 8
}
