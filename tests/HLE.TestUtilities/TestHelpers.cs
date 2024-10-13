using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.TestUtilities;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
public static class TestHelpers
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Consume<T>(T t) => _ = t;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static object Box<T>(T t) where T : struct
        => t;
}
