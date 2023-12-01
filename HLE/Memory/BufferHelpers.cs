using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static class BufferHelpers
{
    /// <summary>
    /// A <i>const</i> mirror of <see cref="Array.MaxLength"/>.
    /// </summary>
    public const int MaximumArrayLength = 0x7FFFFFC7; // keep in sync with Array.MaxLength

    private const uint MaximumPow2Length = 1 << 30;

    /// <summary>
    /// Grows a buffer length by power of 2.
    /// </summary>
    /// <example>
    /// If the current length is 18 and the needed size is 4, the method will return 32 (16 &lt;&lt; 1).<br/>
    /// If the current length is 18 and the needed size is 20, the method will return 64 (16 &lt;&lt; 2).
    /// </example>
    /// <param name="currentLength">The current length of the buffer.</param>
    /// <param name="neededSize">The additionally needed buffer size.</param>
    /// <returns>Returns the new buffer size that satisfies the needed length.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="currentLength"/> or <paramref name="neededSize"/> are negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the maximum capacity, <see cref="Array.MaxLength"/>, will be exceeded.</exception>
    [Pure]
    public static int GrowByPow2(int currentLength, int neededSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentLength);
        ArgumentOutOfRangeException.ThrowIfNegative(neededSize);

        if (neededSize != 0 && currentLength >= MaximumArrayLength)
        {
            ThrowMaximumBufferCapacityReached();
        }

        uint newLength = (uint)currentLength + (uint)neededSize;
        return newLength switch
        {
            <= MaximumPow2Length => (int)BitOperations.RoundUpToPowerOf2(newLength),
            <= MaximumArrayLength => MaximumArrayLength,
            _ => ThrowMaximumBufferCapacityReached()
        };
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ThrowMaximumBufferCapacityReached()
        => throw new InvalidOperationException("The maximum buffer capacity has been reached.");
}
