using System.Diagnostics.CodeAnalysis;

namespace HLE.UnitTests.Marshalling;

public sealed partial class MethodTableTest
{
    [SuppressMessage("Performance", "CA1823:Avoid unused private fields")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    private readonly ref struct GenericRefStructWithRefField<T>
    {
#pragma warning disable CS0169 // Field is never used
        private readonly ref T _t;
#pragma warning restore CS0169 // Field is never used
    }
}
