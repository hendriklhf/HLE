using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
[SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
[SuppressMessage("Critical Code Smell", "S4000:Pointers to unmanaged memory should not be visible")]
public unsafe struct RawArrayData
{
    public MethodTable* MethodTable;

    public nuint Length;

    public byte FirstElement;
}
