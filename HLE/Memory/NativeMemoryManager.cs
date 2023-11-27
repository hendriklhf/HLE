using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;

namespace HLE.Memory;

public sealed class NativeMemoryManager<T>(NativeMemory<T> memory) : MemoryManager<T>, IEquatable<NativeMemoryManager<T>>, ISpanProvider<T>
    where T : unmanaged, IEquatable<T>
{
    private NativeMemory<T> _memory = memory;

    [Pure]
    public override Span<T> GetSpan() => _memory.AsSpan();

    /// <summary>
    /// Throws a <see cref="NotSupportedException"/>.<br/>
    /// The <see cref="NativeMemoryManager{T}"/> manages native memory, thus does not require nor support pinning."
    /// </summary>
    [DoesNotReturn]
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        ThrowNativeMemoryRequiresNoPinning();
        return default;
    }

    /// <summary>
    /// Throws a <see cref="NotSupportedException"/>.<br/>
    /// The <see cref="NativeMemoryManager{T}"/> manages native memory, thus does not require nor support pinning."
    /// </summary>
    [DoesNotReturn]
    public override void Unpin() => ThrowNativeMemoryRequiresNoPinning();

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNativeMemoryRequiresNoPinning()
        => throw new NotSupportedException($"The {typeof(NativeMemoryManager<T>)} manages native memory, thus does not require nor support pinning.");

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memory.Dispose();
        }
    }

    public bool Equals(NativeMemoryManager<T>? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => obj is NativeMemoryManager<T> other && Equals(other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(NativeMemoryManager<T>? left, NativeMemoryManager<T>? right) => Equals(left, right);

    public static bool operator !=(NativeMemoryManager<T>? left, NativeMemoryManager<T>? right) => !(left == right);
}
