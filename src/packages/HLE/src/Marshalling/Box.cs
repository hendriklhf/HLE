using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
[SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
[SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
public readonly unsafe struct Box<T>(ref T value)
{
    private readonly MethodTable* _methodTable = ObjectMarshal.GetMethodTable<T>();
    private readonly T _value = value;
}
