using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Strings;

public static partial class SingleCharStringPool
{
    private static readonly ConcurrentDictionary<char, string> _customSingleCharStringCache = new();

    /// <summary>
    /// Amount of single char strings cached beginning from char '\0'.
    /// </summary>
    /// <remarks>The field is used in the source generator for the static cache. Don't rename.</remarks>
    internal const int AmountOfCachedSingleCharStrings = 2048;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet(char c, [MaybeNullWhen(false)] out string str)
    {
        if (c >= AmountOfCachedSingleCharStrings)
        {
            return _customSingleCharStringCache.TryGetValue(c, out str);
        }

        ReadOnlySpan<string> cachedSingleCharStrings = GetCachedSingleCharStrings();
        ref string reference = ref MemoryMarshal.GetReference(cachedSingleCharStrings);
        str = Unsafe.Add(ref reference, c);
        return true;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetOrAdd(char c)
    {
        if (TryGet(c, out string? str))
        {
            return str;
        }

        str = c.ToString();
        _customSingleCharStringCache.AddOrSet(c, str);
        return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(string str)
    {
        if (str.Length != 1)
        {
            return;
        }

        char c = str[0];
        if (c < AmountOfCachedSingleCharStrings)
        {
            return;
        }

        _customSingleCharStringCache.AddOrSet(c, str);
    }

    public static bool Contains(char c) => c < AmountOfCachedSingleCharStrings || _customSingleCharStringCache.ContainsKey(c);

    internal static partial ReadOnlySpan<string> GetCachedSingleCharStrings();
}
