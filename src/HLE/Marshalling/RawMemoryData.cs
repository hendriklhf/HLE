using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
public struct RawMemoryData
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "not the type name")]
    public nuint Object;

    public int Index;

    public int Length;
}
