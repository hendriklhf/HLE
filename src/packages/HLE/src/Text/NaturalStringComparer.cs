using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Numerics;

namespace HLE.Text;

internal sealed class NaturalStringComparer : IComparer<string?>, IComparer, IEquatable<NaturalStringComparer>
{
    private readonly StringComparer _comparer = StringComparer.Ordinal;

    private static readonly SearchValues<char> s_digitSearchValues = SearchValues.Create("0123456789");

    public int Compare(object? x, object? y)
        => Compare(x as string, y as string);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(string? x, string? y)
    {
        if (x is null)
        {
            return y is null ? 0 : -1;
        }

        if (y is null)
        {
            return 1;
        }

        if (x.Length == 0)
        {
            return y.Length == 0 ? 0 : -1;
        }

        if (y.Length == 0)
        {
            return 1;
        }

        return CompareCore(x, y);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private int CompareCore(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        Debug.Assert(x.Length != 0);
        Debug.Assert(y.Length != 0);

        int start = SpanHelpers.IndexOfDifference(x, y);
        if (start == -1)
        {
            if (x.Length == y.Length)
            {
                return 0;
            }

            return x.Length < y.Length ? -1 : 1;
        }

        x = x.SliceUnsafe(start);
        y = y.SliceUnsafe(start);

        if (char.IsAsciiDigit(x[0]))
        {
            return CompareWithNumbers(x, y);
        }

        if (char.IsAsciiDigit(y[0]))
        {
#pragma warning disable S2234
            return -CompareWithNumbers(y, x);
#pragma warning restore S2234
        }

        return NormalCompare(x, y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private unsafe int NormalCompare(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        nuint xSize = ObjectMarshal.GetRawStringSize(x.Length);
        nuint size = xSize + ObjectMarshal.GetRawStringSize(y.Length) + 16;
        byte[] buffer = Memory.ArrayPool<byte>.Shared.Rent(int.CreateChecked(size));

        ref RawStringData rawX = ref StringAllocator.Alloc(buffer, x);
        nuint alignedXSize = NumberHelpers.Align(xSize, (uint)sizeof(nuint), AlignmentMethod.Add);
        ref RawStringData rawY = ref StringAllocator.Alloc(buffer.AsSpan(int.CreateChecked(alignedXSize)), y);

        int result = _comparer.Compare(ObjectMarshal.GetString(ref rawX), ObjectMarshal.GetString(ref rawY));
        Memory.ArrayPool<byte>.Shared.Return(buffer);
        return result;
    }

    private static int CompareWithNumbers(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        // x starts with a digit
        // y might start with a digit or not

        Debug.Assert(char.IsAsciiDigit(x[0]));

        if (!char.IsAsciiDigit(y[0]))
        {
            return -1;
        }

        int numberEndX = x.IndexOfAnyExcept(s_digitSearchValues);
        int numberEndY = y.IndexOfAnyExcept(s_digitSearchValues);

        ulong numberX = ulong.Parse(x.SliceUnsafe(..Unsafe.BitCast<int, Index>(numberEndX)));
        ulong numberY = ulong.Parse(y.SliceUnsafe(..Unsafe.BitCast<int, Index>(numberEndY)));

        if (numberX != numberY)
        {
            return numberX < numberY ? -1 : 1;
        }

        return 0;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] NaturalStringComparer? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(NaturalStringComparer? left, NaturalStringComparer? right) => Equals(left, right);

    public static bool operator !=(NaturalStringComparer? left, NaturalStringComparer? right) => !(left == right);
}
