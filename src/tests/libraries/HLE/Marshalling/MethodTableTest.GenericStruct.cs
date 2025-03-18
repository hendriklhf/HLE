using System.Diagnostics.CodeAnalysis;

namespace HLE.UnitTests.Marshalling;

public sealed partial class MethodTableTest
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    [SuppressMessage("Performance", "CA1823:Avoid unused private fields")]
    private readonly struct GenericStruct<T>
    {
#pragma warning disable CS0169 // Field is never used
        private readonly T _t;
#pragma warning restore CS0169 // Field is never used
    }
}
