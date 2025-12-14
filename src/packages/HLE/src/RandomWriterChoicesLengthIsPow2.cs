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

internal sealed class RandomWriterChoicesLengthIsPow2 : RandomWriter, IEquatable<RandomWriterChoicesLengthIsPow2>
{
    public override void Write<T>(Random random, ref T destination, int destinationLength, ref T choices, int choicesLength)
    {
        Debug.Assert(BitOperations.IsPow2(choicesLength));

        int mask = choicesLength - 1;
        if (!MemoryHelpers.UseStackalloc<uint>(destinationLength))
        {
            uint[] randomIndicesBuffer = ArrayPool<uint>.Shared.Rent(destinationLength);
            random.Fill(randomIndicesBuffer.AsSpan(..destinationLength));
            ref uint indicesBufferRef = ref MemoryMarshal.GetArrayDataReference(randomIndicesBuffer);
            for (int i = 0; i < destinationLength; i++)
            {
                int randomIndex = (int)(Unsafe.Add(ref indicesBufferRef, i) & mask);
                Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
            }

            ArrayPool<uint>.Shared.Return(randomIndicesBuffer);

            return;
        }

        Span<uint> randomIndices = stackalloc uint[destinationLength];
        random.Fill(randomIndices);
        ref uint indicesRef = ref MemoryMarshal.GetReference(randomIndices);
        for (int i = 0; i < destinationLength; i++)
        {
            int randomIndex = (int)(Unsafe.Add(ref indicesRef, i) & mask);
            Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] RandomWriterChoicesLengthIsPow2? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RandomWriterChoicesLengthIsPow2? left, RandomWriterChoicesLengthIsPow2? right) => Equals(left, right);

    public static bool operator !=(RandomWriterChoicesLengthIsPow2? left, RandomWriterChoicesLengthIsPow2? right) => !(left == right);
}
