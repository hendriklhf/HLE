using System;
using System.Diagnostics.Contracts;
using HLE.Text;

namespace HLE.UnitTests.Text;

public sealed partial class LazyStringTest
{
#pragma warning disable CA1815
    public readonly struct Parameter(string value, Func<LazyString> lazy)
    {
        public string Value { get; } = value;

        private readonly Func<LazyString> _lazy = lazy;

        [Pure]
        public LazyString CreateLazy() => _lazy();

        public override string ToString() => $"\"{Value}\"";
    }
#pragma warning restore CA1815
}
