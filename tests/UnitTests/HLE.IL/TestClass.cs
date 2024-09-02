using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.IL.UnitTests;

[StructLayout(LayoutKind.Explicit)]
[SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
internal sealed class TestClass(string str, int value)
{
    [FieldOffset(0)]
    private readonly string _str = str;

    [FieldOffset(8)]
    private readonly int _int = value;
}
