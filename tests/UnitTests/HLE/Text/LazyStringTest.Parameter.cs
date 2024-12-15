using HLE.Text;

namespace HLE.UnitTests.Text;

public sealed partial class LazyStringTest
{
#pragma warning disable CA1815
    public readonly struct Parameter(string value, LazyString lazy)
    {
        public string Value { get; } = value;

        public LazyString Lazy { get; } = lazy;
    }
#pragma warning restore CA1815
}
