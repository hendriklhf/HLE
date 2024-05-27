using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Memory;

public struct FrozenSegmentHandle : IDisposable, IEquatable<FrozenSegmentHandle>
{
    public readonly bool IsValid => _handle != 0;

    internal nint Value
    {
        readonly get
        {
            if (!IsValid)
            {
                ThrowHelper.ThrowObjectDisposedException<FrozenSegmentHandle>();
            }

            return _handle;
        }
        private set => _handle = value;
    }

    private nint _handle;

    internal FrozenSegmentHandle(nint handle) => Value = handle;

    public void Dispose() => Value = 0;

    [Pure]
    public readonly bool Equals(FrozenSegmentHandle other) => Value == other.Value;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FrozenSegmentHandle other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(FrozenSegmentHandle left, FrozenSegmentHandle right) => left.Equals(right);

    public static bool operator !=(FrozenSegmentHandle left, FrozenSegmentHandle right) => !(left == right);
}
