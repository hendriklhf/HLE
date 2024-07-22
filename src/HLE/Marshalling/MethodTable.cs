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

    public bool IsValueType => (_flags & IsValueTypeFlag) != 0;

    public bool ContainsManagedPointers => (_flags & ContainsManagedPointersFlag) != 0;

    public bool IsReferenceOrContainsReferences => !IsValueType || ContainsManagedPointers;

    [FieldOffset(0)]
    private readonly ushort _componentSize;

    [FieldOffset(0)]
    private readonly uint _flags;

    private const uint IsValueTypeFlag = 0x00040000;
    private const uint ContainsManagedPointersFlag = 0x1000000;
}
