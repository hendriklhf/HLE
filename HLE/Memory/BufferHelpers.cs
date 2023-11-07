using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static class BufferHelpers
{
    private const uint _maximumPow2Length = 1 << 30;

    [Pure]
    public static int GrowByPow2(int currentLength, int neededSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentLength);
        ArgumentOutOfRangeException.ThrowIfNegative(neededSize);

        if (neededSize != 0 && currentLength == int.MaxValue)
        {
            ThrowMaximumBufferCapacityReached();
        }

        uint newLength = (uint)currentLength + (uint)neededSize;
        return newLength switch
        {
            <= _maximumPow2Length => (int)BitOperations.RoundUpToPowerOf2(newLength),
            <= int.MaxValue => int.MaxValue,
            _ => ThrowMaximumBufferCapacityReached()
        };
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ThrowMaximumBufferCapacityReached()
        => throw new InvalidOperationException("The maximum buffer capacity has been reached.");
}
