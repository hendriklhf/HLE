using System.Diagnostics.CodeAnalysis;

namespace HLE.UnitTests;

public sealed partial class EnumValuesTest
{
    [SuppressMessage("Roslynator", "RCS1154:Sort enum members", Justification = "not sorted for testing purposes")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum TestEnum
    {
        Y = 1000,
        Z = -50,
        A = 50,
        B = 35,
        C = 0,
        W = -2,
        D = 12345,
        E = 55,
        F = 1,
        G = 3,
        H = 8,
        I = -100
    }
}
