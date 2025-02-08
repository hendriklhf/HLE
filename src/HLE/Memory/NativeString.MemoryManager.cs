using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public readonly partial struct NativeString
{
    private sealed class MemoryManager(NativeString str) : MemoryManager<char>, IEquatable<MemoryManager>
    {
        private readonly NativeString _str = str;

        protected override void Dispose(bool disposing) => _str.Dispose();

        public override Span<char> GetSpan() => _str.AsSpan();

        public override MemoryHandle Pin(int elementIndex = 0) => throw new NotSupportedException();

        public override void Unpin() => throw new NotSupportedException();

        public bool Equals([NotNullWhen(true)] MemoryManager? other) => ReferenceEquals(this, other);

        public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

        public static bool operator ==(MemoryManager? left, MemoryManager? right) => Equals(left, right);

        public static bool operator !=(MemoryManager? left, MemoryManager? right) => !Equals(left, right);
    }
}
