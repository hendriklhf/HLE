using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory.UnitTests;

public sealed partial class InlineArrayHelpersTest
{
    [InlineArray(Length)]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
    [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
    [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
    [SuppressMessage("Roslynator", "RCS1158:Static member in generic type should use a type parameter")]
    private struct SomeInlineArray<T>
    {
        private T _item;

        public const int Length = 8;
    }
}
