using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.Text;

/// <summary>
/// Used to quickly hash a string, as this algorithm executes in constant time, because it doesn't depend on the string's length.
/// For example, used for <see cref="StringPool"/> and <see cref="RegexPool"/> to quickly find a bucket index for a string.
/// </summary>
internal static class SimpleStringHasher
{
    private static readonly uint s_seed = Random.Shared.NextUInt32();

    [Pure]
    public static uint Hash(ref PooledInterpolatedStringHandler chars)
    {
        uint hash = Hash(chars.Text);
        chars.Dispose();
        return hash;
    }

    public static uint Hash(string chars) => Hash(ref StringMarshal.GetReference(chars), (uint)chars.Length);

    [Pure]
    public static uint Hash(ReadOnlySpan<char> chars) => Hash(ref MemoryMarshal.GetReference(chars), (uint)chars.Length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash(ref char chars, uint length)
    {
        const int CharBitCount = sizeof(char) * 8;

        if (length == 0)
        {
            return 0;
        }

        char firstChar = chars;
        char middleChar = Unsafe.Add(ref chars, length >> 1);
        char lastChar = Unsafe.Add(ref chars, length - 1);

        uint hash = (uint)(~(firstChar | (firstChar << CharBitCount)) ^ ~(middleChar | (middleChar << CharBitCount)) ^ ~(lastChar | (lastChar << CharBitCount)));
        hash ^= (length | (length << CharBitCount));
        return hash ^ s_seed;
    }
}
