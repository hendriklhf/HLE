using System;

namespace HLE.Marshalling.Windows;

[Flags]
public enum MouseInputFlags
{
    None = 0,
    MovementOccured = 0x1,
    LeftButtonPressed = 0x2,
    LeftButtonReleased = 0x4,
    RightButtonPressed = 0x8,
    RightButtonReleased = 0x10,
    MiddleButtonPressed = 0x20,
    MiddleButtonReleased = 0x40,
    XButtonPressed = 0x80,
    XButtonReleased = 0x100,
    MouseWheelMoved = 0x800,
    MouseWheelMovedHorizontally = 0x1000,
    DontCoalesceMouseMoveMessages = 0x2000,
    MapCoordinatesToEntireDesktop = 0x4000,
    AbsoluteCoordinates = 0x8000
}
