using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE;

internal class RandomFiller : IEquatable<RandomFiller>
{
    private static readonly RandomFiller s_fallBackFiller = new();
    private static readonly RandomFillerChoicesLengthIsPow2 s_fillerChoicesLengthIsPow2 = new();
    private static readonly RandomFillerChoicesLengthIs8Bits s_fillerChoicesLengthIs8Bits = new();
    private static readonly RandomFillerChoicesLengthIsPow2And8Bits s_fillerChoicesLengthIsPow2And8Bits = new();
    private static readonly RandomFillerChoicesLengthIs16Bits s_fillerChoicesLengthIs16Bits = new();
    private static readonly RandomFillerChoicesLengthIsPow2And16Bits s_fillerChoicesLengthIsPow2And16Bits = new();

    protected RandomFiller()
    {
    }

    [Pure]
    public static RandomFiller Create(int choicesLength)
    {
        RandomFillingOptimizations optimizations = RandomFillingOptimizations.None;
        if (BitOperations.IsPow2(choicesLength))
        {
            optimizations |= RandomFillingOptimizations.ChoicesLengthIsPow2;
        }

        if (choicesLength <= ushort.MaxValue)
        {
            optimizations |= choicesLength <= byte.MaxValue
                ? RandomFillingOptimizations.ChoicesLengthIs8Bits
                : RandomFillingOptimizations.ChoicesLengthIs16Bits;
        }

        return Create(optimizations);
    }

    [Pure]
    public static RandomFiller Create(RandomFillingOptimizations optimizations) => optimizations switch
    {
        RandomFillingOptimizations.None => s_fallBackFiller,
        RandomFillingOptimizations.ChoicesLengthIsPow2 => s_fillerChoicesLengthIsPow2,
        RandomFillingOptimizations.ChoicesLengthIs8Bits => s_fillerChoicesLengthIs8Bits,
        RandomFillingOptimizations.ChoicesLengthIsPow2 | RandomFillingOptimizations.ChoicesLengthIs8Bits => s_fillerChoicesLengthIsPow2And8Bits,
        RandomFillingOptimizations.ChoicesLengthIs16Bits => s_fillerChoicesLengthIs16Bits,
        RandomFillingOptimizations.ChoicesLengthIsPow2 | RandomFillingOptimizations.ChoicesLengthIs16Bits => s_fillerChoicesLengthIsPow2And16Bits,
        RandomFillingOptimizations.ChoicesLengthIs8Bits | RandomFillingOptimizations.ChoicesLengthIs16Bits => s_fillerChoicesLengthIs16Bits,
        RandomFillingOptimizations.All => s_fillerChoicesLengthIsPow2And16Bits,
        _ => s_fallBackFiller
    };

    [SkipLocalsInit]
    public virtual void Fill<T>(Random random, ref T destination, int destinationLength, ref T choices, int choicesLength)
    {
        if (!MemoryHelpers.UseStackalloc<uint>(destinationLength))
        {
            using RentedArray<uint> randomIndicesBuffer = ArrayPool<uint>.Shared.RentAsRentedArray(destinationLength);
            random.Fill(randomIndicesBuffer.AsSpan(..destinationLength));
            ref uint indicesBufferRef = ref randomIndicesBuffer.Reference;
            for (int i = 0; i < destinationLength; i++)
            {
                int randomIndex = (int)(Unsafe.Add(ref indicesBufferRef, i) % choicesLength);
                Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
            }

            return;
        }

        Span<uint> randomIndices = stackalloc uint[destinationLength];
        random.Fill(randomIndices);
        ref uint indicesRef = ref MemoryMarshal.GetReference(randomIndices);
        for (int i = 0; i < destinationLength; i++)
        {
            int randomIndex = (int)(Unsafe.Add(ref indicesRef, i) % choicesLength);
            Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] RandomFiller? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RandomFiller? left, RandomFiller? right) => Equals(left, right);

    public static bool operator !=(RandomFiller? left, RandomFiller? right) => !(left == right);
}
