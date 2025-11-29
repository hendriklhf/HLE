using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed unsafe class NativeMemoryManager<T>(T* memory, int length) :
    MemoryManager<T>,
    IEquatable<NativeMemoryManager<T>>
    where T : unmanaged, IEquatable<T>
{
    private readonly T* _memory = memory;
    private readonly int _length = length;

    [Pure]
    public override Span<T> GetSpan() => new(_memory, _length);

    public override MemoryHandle Pin(int elementIndex = 0) => new(_memory + elementIndex);

    public override void Unpin()
    {
        // noop
    }

    protected override void Dispose(bool disposing)
    {
    }

    public bool Equals([NotNullWhen(true)] NativeMemoryManager<T>? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is NativeMemoryManager<T> other && Equals(other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(NativeMemoryManager<T>? left, NativeMemoryManager<T>? right) => Equals(left, right);

    public static bool operator !=(NativeMemoryManager<T>? left, NativeMemoryManager<T>? right) => !(left == right);
}
