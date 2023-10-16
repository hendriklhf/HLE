using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Strings;

/// <summary>
/// Used to quickly hash a string, as this algorithm executes in constant time, because it doesn't depend on the string's length.
/// </summary>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct SimpleStringHasher(ReadOnlySpan<char> chars)
{
    private readonly ReadOnlySpan<char> _chars = chars;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Hash() => Hash(_chars);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Hash(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            return 0;
        }

        int length = chars.Length;
        ref char firstChar = ref MemoryMarshal.GetReference(chars);
        char middleChar = Unsafe.Add(ref firstChar, length >> 1);
        char lastChar = Unsafe.Add(ref firstChar, length - 1);

        int hash = ~(firstChar | (firstChar << 16)) ^ ~(middleChar | (middleChar << 16)) ^ ~(lastChar | (lastChar << 16));
        return hash ^ (length | (length << 16));
    }
}
