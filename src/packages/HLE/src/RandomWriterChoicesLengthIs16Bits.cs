using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE;

internal sealed class RandomWriterChoicesLengthIs16Bits : RandomWriter, IEquatable<RandomWriterChoicesLengthIs16Bits>
{
    public override void Write<T>(Random random, ref T destination, int destinationLength, ref T choices, int choicesLength)
    {
        Debug.Assert(choicesLength <= ushort.MaxValue);
        Debug.Assert(!BitOperations.IsPow2(choicesLength));

        if (!MemoryHelpers.UseStackalloc<ushort>(destinationLength))
        {
            ushort[] randomIndicesBuffer = ArrayPool<ushort>.Shared.Rent(destinationLength);
            random.Fill(randomIndicesBuffer.AsSpan(..destinationLength));
            ref ushort indicesBufferRef = ref MemoryMarshal.GetArrayDataReference(randomIndicesBuffer);
            for (int i = 0; i < destinationLength; i++)
            {
                int randomIndex = Unsafe.Add(ref indicesBufferRef, i) % choicesLength;
                Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
            }

            ArrayPool<ushort>.Shared.Return(randomIndicesBuffer);

            return;
        }

        Span<ushort> randomIndices = stackalloc ushort[destinationLength];
        random.Fill(randomIndices);
        ref ushort indicesRef = ref MemoryMarshal.GetReference(randomIndices);
        for (int i = 0; i < destinationLength; i++)
        {
            int randomIndex = Unsafe.Add(ref indicesRef, i) % choicesLength;
            Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] RandomWriterChoicesLengthIs16Bits? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RandomWriterChoicesLengthIs16Bits? left, RandomWriterChoicesLengthIs16Bits? right) => Equals(left, right);

    public static bool operator !=(RandomWriterChoicesLengthIs16Bits? left, RandomWriterChoicesLengthIs16Bits? right) => !(left == right);
}
