using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
[SuppressMessage("Critical Code Smell", "S4000:Pointers to unmanaged memory should not be visible")]
[SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
public unsafe struct RawStringData
{
    public MethodTable* MethodTable;

    public int Length;

    public char FirstChar;
}
