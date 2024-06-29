using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Explicit)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
[SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
public readonly struct MethodTable
{
    /// <summary>
    /// Gets the size of each element in an <see cref="Array"/> or <see cref="string"/>.
    /// </summary>
    public ushort ComponentSize => _componentSize;

    public bool ContainsManagedPointers => (_flags & 0x01000000) != 0;

    [FieldOffset(0)]
    private readonly ushort _componentSize;

    [FieldOffset(0)]
    private readonly uint _flags;
}
