using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE;

internal sealed class RandomWriterChoicesLengthIs8Bits : RandomWriter, IEquatable<RandomWriterChoicesLengthIs8Bits>
{
    [SkipLocalsInit]
    public override void Write<T>(Random random, ref T destination, int destinationLength, ref T choices, int choicesLength)
    {
        Debug.Assert(choicesLength <= byte.MaxValue);
        Debug.Assert(!BitOperations.IsPow2(choicesLength));

        if (!MemoryHelpers.UseStackalloc<byte>(destinationLength))
        {
            using RentedArray<byte> randomIndicesBuffer = ArrayPool<byte>.Shared.RentAsRentedArray(destinationLength);
            random.Fill(randomIndicesBuffer.AsSpan(..destinationLength));
            ref byte indicesBufferRef = ref randomIndicesBuffer.Reference;
            for (int i = 0; i < destinationLength; i++)
            {
                int randomIndex = Unsafe.Add(ref indicesBufferRef, i) % choicesLength;
                Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
            }

            return;
        }

        Span<byte> randomIndices = stackalloc byte[destinationLength];
        random.Fill(randomIndices);
        ref byte indicesRef = ref MemoryMarshal.GetReference(randomIndices);
        for (int i = 0; i < destinationLength; i++)
        {
            int randomIndex = Unsafe.Add(ref indicesRef, i) % choicesLength;
            Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] RandomWriterChoicesLengthIs8Bits? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RandomWriterChoicesLengthIs8Bits? left, RandomWriterChoicesLengthIs8Bits? right) => Equals(left, right);

    public static bool operator !=(RandomWriterChoicesLengthIs8Bits? left, RandomWriterChoicesLengthIs8Bits? right) => !(left == right);
}
