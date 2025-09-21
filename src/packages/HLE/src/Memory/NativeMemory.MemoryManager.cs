using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Memory;

public sealed partial class NativeMemory<T>
{
    private sealed class MemoryManager(NativeMemory<T> memory) : MemoryManager<T>, IEquatable<MemoryManager>
    {
        private readonly NativeMemory<T> _memory = memory;

        protected override void Dispose(bool disposing) => _memory.Dispose();

        public override Span<T> GetSpan() => _memory.AsSpan();

        [DoesNotReturn]
        public override MemoryHandle Pin(int elementIndex = 0) => throw new NotSupportedException();

        [DoesNotReturn]
        public override void Unpin() => throw new NotSupportedException();

        [Pure]
        public bool Equals([NotNullWhen(true)] MemoryManager? other) => ReferenceEquals(this, other);

        [Pure]
        public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

        [Pure]
        public override int GetHashCode() => _memory.GetHashCode();

        public static bool operator ==(MemoryManager? left, MemoryManager? right) => Equals(left, right);

        public static bool operator !=(MemoryManager? left, MemoryManager? right) => !Equals(left, right);
    }
}
