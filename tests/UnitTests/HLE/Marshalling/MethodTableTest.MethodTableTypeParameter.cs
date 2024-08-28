using System;

namespace HLE.UnitTests.Marshalling;

public sealed partial class MethodTableTest
{
    public sealed class MethodTableTypeParameter(Type type, ushort componentSize, bool containsManagedPointers)
    {
        public ushort ComponentSize { get; } = componentSize;

        public bool ContainsManagedPointers { get; } = containsManagedPointers;

        public Type Type { get; } = type;

        public override string ToString() => Type.ToString();
    }
}
