using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static class BufferHelpers
{
    private static readonly nuint s_maximumNativePow2Length = Environment.Is64BitProcess ? unchecked((nuint)MaximumUInt64Pow2Length) : MaximumUInt32Pow2Length;

    /// <summary>
    /// A <see langword="const" /> mirror of <see cref="Array"/>.<see cref="Array.MaxLength"/>.
    /// </summary>
    public const int MaximumArrayLength = 0x7FFFFFC7; // keep in sync with Array.MaxLength

    private const uint MaximumUInt32Pow2Length = 1 << 30;
    private const ulong MaximumUInt64Pow2Length = 1UL << 62;

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
    public static int GrowArray(uint currentLength, uint neededSize)
    {
        if (neededSize != 0 && currentLength >= MaximumArrayLength)
        {
            ThrowMaximumBufferCapacityReached<int>();
        }

        uint newLength = checked(currentLength + neededSize);
        return newLength switch
        {
            <= MaximumUInt32Pow2Length => (int)BitOperations.RoundUpToPowerOf2(newLength),
            <= MaximumArrayLength => MaximumArrayLength,
            _ => ThrowMaximumBufferCapacityReached<int>()
        };
    }

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
    /// <exception cref="InvalidOperationException">Thrown if the maximum capacity, <see cref="int.MaxValue"/>, will be exceeded.</exception>
    [Pure]
    public static int GrowBuffer(uint currentLength, uint neededSize)
    {
        if (neededSize != 0 && currentLength >= int.MaxValue)
        {
            ThrowMaximumBufferCapacityReached<int>();
        }

        uint newLength = checked(currentLength + neededSize);
        return newLength switch
        {
            <= MaximumUInt32Pow2Length => (int)BitOperations.RoundUpToPowerOf2(newLength),
            <= int.MaxValue => int.MaxValue,
            _ => ThrowMaximumBufferCapacityReached<int>()
        };
    }

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
    /// <exception cref="OverflowException">Thrown if the maximum capacity, <see cref="nuint.MaxValue"/>, will be exceeded.</exception>
    [Pure]
    public static nuint GrowNativeBuffer(nuint currentLength, nuint neededSize)
    {
        nuint newLength = checked(currentLength + neededSize);
        return newLength <= s_maximumNativePow2Length ? BitOperations.RoundUpToPowerOf2(newLength) : nuint.MaxValue;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowMaximumBufferCapacityReached<T>()
        => throw new InvalidOperationException("The maximum buffer capacity has been reached.");
}
