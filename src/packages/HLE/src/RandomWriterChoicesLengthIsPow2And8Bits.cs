using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE;

internal sealed class RandomWriterChoicesLengthIsPow2And8Bits : RandomWriter, IEquatable<RandomWriterChoicesLengthIsPow2And8Bits>
{
    public override void Write<T>(Random random, ref T destination, int destinationLength, ref T choices, int choicesLength)
    {
        Debug.Assert(BitOperations.IsPow2(choicesLength));
        Debug.Assert(choicesLength <= byte.MaxValue);

        int mask = choicesLength - 1;
        if (!MemoryHelpers.UseStackalloc<byte>(destinationLength))
        {
            byte[] randomIndicesBuffer = ArrayPool<byte>.Shared.Rent(destinationLength);
            random.Fill(randomIndicesBuffer.AsSpan(..destinationLength));
            ref byte indicesBufferRef = ref MemoryMarshal.GetArrayDataReference(randomIndicesBuffer);
            for (int i = 0; i < destinationLength; i++)
            {
                int randomIndex = Unsafe.Add(ref indicesBufferRef, i) & mask;
                Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
            }

            ArrayPool<byte>.Shared.Return(randomIndicesBuffer);

            return;
        }

        Span<byte> randomIndices = stackalloc byte[destinationLength];
        random.Fill(randomIndices);
        ref byte indicesRef = ref MemoryMarshal.GetReference(randomIndices);
        for (int i = 0; i < destinationLength; i++)
        {
            int randomIndex = Unsafe.Add(ref indicesRef, i) & mask;
            Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] RandomWriterChoicesLengthIsPow2And8Bits? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RandomWriterChoicesLengthIsPow2And8Bits? left, RandomWriterChoicesLengthIsPow2And8Bits? right) => Equals(left, right);

    public static bool operator !=(RandomWriterChoicesLengthIsPow2And8Bits? left, RandomWriterChoicesLengthIsPow2And8Bits? right) => !(left == right);
}
