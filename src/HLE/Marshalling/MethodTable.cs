using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

[StructLayout(LayoutKind.Explicit)]
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
public readonly struct MethodTable
{
    /// <summary>
    /// Gets the size of each element in an <see cref="Array"/> or <see cref="string"/>.
    /// </summary>
    public ushort ComponentSize => _componentSize;

    [FieldOffset(0)]
    private readonly ushort _componentSize;
}
