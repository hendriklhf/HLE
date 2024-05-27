using System.Diagnostics.CodeAnalysis;

namespace HLE.Tests;

public sealed partial class EnumValuesTest
{
    [SuppressMessage("Roslynator", "RCS1154:Sort enum members", Justification = "not sorted for testing purposes")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum TestEnum
    {
        A = 50,
        B = 35,
        C = 0,
        D = 12345,
        E = 55,
        F = 1,
        G = 3,
        H = 8
    }
}
