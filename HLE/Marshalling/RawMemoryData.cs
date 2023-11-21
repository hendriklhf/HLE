using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
public struct RawMemoryData
{
    public nuint Reference;

    public int Index;

    public int Length;
}
