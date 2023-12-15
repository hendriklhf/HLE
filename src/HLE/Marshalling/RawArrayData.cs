using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
public struct RawArrayData
{
    public nuint MethodTablePointer;

    public nuint Length;

    public byte FirstElement;
}
