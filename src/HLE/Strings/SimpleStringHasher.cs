using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Strings;

/// <summary>
/// Used to quickly hash a string, as this algorithm executes in constant time, because it doesn't depend on the string's length.
/// </summary>
/// <param name="chars">The chars of which the hashcode will be created from.</param>
internal readonly ref struct SimpleStringHasher(ReadOnlySpan<char> chars)
{
    private readonly ref char _chars = ref MemoryMarshal.GetReference(chars);
    private readonly int _length = chars.Length;

    private static readonly uint s_seed = Random.Shared.NextUInt32();

    public uint Hash() => Hash(ref _chars, _length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash(ReadOnlySpan<char> chars) => Hash(ref MemoryMarshal.GetReference(chars), chars.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash(ref char chars, int length)
    {
        const int CharBitCount = sizeof(char) * 8;

        if (length == 0)
        {
            return 0;
        }

        char middleChar = Unsafe.Add(ref chars, length >>> 1);
        char lastChar = Unsafe.Add(ref chars, length - 1);

        uint hash = (uint)(~(chars | (chars << CharBitCount)) ^ ~(middleChar | (middleChar << CharBitCount)) ^ ~(lastChar | (lastChar << CharBitCount)));
        hash ^= (uint)(length | (length << CharBitCount));
        return hash ^ s_seed;
    }
}
