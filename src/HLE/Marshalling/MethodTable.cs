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

    public bool IsValueType => (_flags & ValueTypeMask) == IsValueTypeFlag;

    // ReSharper disable once InconsistentNaming
    public bool ContainsGCPointers => (_flags & ContainsGCPointersFlag) != 0;

    public bool IsInterface => (_flags & CategoryMask) == IsInterfaceFlag;

    public bool HasComponentSize => (_flags & HasComponentSizeFlag) != 0;

    [FieldOffset(0)]
    private readonly ushort _componentSize;

    [FieldOffset(0)]
    private readonly uint _flags;

    private const uint IsValueTypeFlag = 0x00040000;
    private const uint ValueTypeMask = 0x000C0000;
    private const uint IsInterfaceFlag = 0x000C0000;
    private const uint CategoryMask = 0x000F0000;
    // ReSharper disable once InconsistentNaming
    private const uint ContainsGCPointersFlag = 0x1000000;
    private const uint HasComponentSizeFlag = 0x80000000;
}
