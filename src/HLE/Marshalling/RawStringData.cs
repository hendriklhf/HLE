using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
public unsafe struct RawStringData
{
    public MethodTable* MethodTable;

    public int Length;

    public char FirstChar;
}
