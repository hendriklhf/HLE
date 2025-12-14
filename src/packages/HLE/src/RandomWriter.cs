using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE;

internal class RandomWriter : IEquatable<RandomWriter>
{
    private static readonly RandomWriter s_fallbackWriter = new();
    private static readonly RandomWriterChoicesLengthIsPow2 s_writerChoicesLengthIsPow2 = new();
    private static readonly RandomWriterChoicesLengthIs8Bits s_writerChoicesLengthIs8Bits = new();
    private static readonly RandomWriterChoicesLengthIsPow2And8Bits s_writerChoicesLengthIsPow2And8Bits = new();
    private static readonly RandomWriterChoicesLengthIs16Bits s_writerChoicesLengthIs16Bits = new();
    private static readonly RandomWriterChoicesLengthIsPow2And16Bits s_writerChoicesLengthIsPow2And16Bits = new();

    protected RandomWriter()
    {
    }

    [Pure]
    public static RandomWriter Create(int choicesLength)
    {
        RandomWritingOptimizations optimizations = RandomWritingOptimizations.None;
        if (BitOperations.IsPow2(choicesLength))
        {
            optimizations |= RandomWritingOptimizations.ChoicesLengthIsPow2;
        }

        if (choicesLength <= ushort.MaxValue)
        {
            optimizations |= choicesLength <= byte.MaxValue
                ? RandomWritingOptimizations.ChoicesLengthIs8Bits
                : RandomWritingOptimizations.ChoicesLengthIs16Bits;
        }

        return Create(optimizations);
    }

    [Pure]
    public static RandomWriter Create(RandomWritingOptimizations optimizations) => optimizations switch
    {
        RandomWritingOptimizations.None => s_fallbackWriter,
        RandomWritingOptimizations.ChoicesLengthIsPow2 => s_writerChoicesLengthIsPow2,
        RandomWritingOptimizations.ChoicesLengthIs8Bits => s_writerChoicesLengthIs8Bits,
        RandomWritingOptimizations.ChoicesLengthIsPow2 | RandomWritingOptimizations.ChoicesLengthIs8Bits => s_writerChoicesLengthIsPow2And8Bits,
        RandomWritingOptimizations.ChoicesLengthIs16Bits => s_writerChoicesLengthIs16Bits,
        RandomWritingOptimizations.ChoicesLengthIsPow2 | RandomWritingOptimizations.ChoicesLengthIs16Bits => s_writerChoicesLengthIsPow2And16Bits,
        RandomWritingOptimizations.ChoicesLengthIs8Bits | RandomWritingOptimizations.ChoicesLengthIs16Bits => s_writerChoicesLengthIs16Bits,
        RandomWritingOptimizations.All => s_writerChoicesLengthIsPow2And16Bits,
        _ => s_fallbackWriter
    };

    public virtual void Write<T>(Random random, ref T destination, int destinationLength, ref T choices, int choicesLength)
    {
        if (!MemoryHelpers.UseStackalloc<uint>(destinationLength))
        {
            uint[] randomIndicesBuffer = ArrayPool<uint>.Shared.Rent(destinationLength);
            random.Fill(randomIndicesBuffer.AsSpan(..destinationLength));
            ref uint indicesBufferRef = ref MemoryMarshal.GetArrayDataReference(randomIndicesBuffer);
            for (int i = 0; i < destinationLength; i++)
            {
                int randomIndex = (int)(Unsafe.Add(ref indicesBufferRef, i) % choicesLength);
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
            int randomIndex = (int)(Unsafe.Add(ref indicesRef, i) % choicesLength);
            Unsafe.Add(ref destination, i) = Unsafe.Add(ref choices, randomIndex);
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] RandomWriter? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RandomWriter? left, RandomWriter? right) => Equals(left, right);

    public static bool operator !=(RandomWriter? left, RandomWriter? right) => !(left == right);
}
