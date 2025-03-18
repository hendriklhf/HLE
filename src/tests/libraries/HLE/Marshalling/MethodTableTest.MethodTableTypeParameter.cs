using System;

namespace HLE.UnitTests.Marshalling;

public sealed partial class MethodTableTest
{
    // ReSharper disable once InconsistentNaming
    public sealed class MethodTableTypeParameter(Type type, ushort componentSize, bool containsGCPointers)
    {
        public ushort ComponentSize { get; } = componentSize;

        // ReSharper disable once InconsistentNaming
        public bool ContainsGCPointers { get; } = containsGCPointers;

        public Type Type { get; } = type;

        public override string ToString() => Type.ToString();
    }
}
