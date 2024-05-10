using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;

namespace HLE.Strings;

public static partial class SingleCharStringPool
{
    private static readonly ConcurrentDictionary<char, string> s_customSingleCharStringCache = new();

    /// <summary>
    /// Amount of single char strings cached beginning from char '\0'.
    /// </summary>
    /// <remarks>The field is used in the source generator for the static cache. Don't rename.</remarks>
    internal const int AmountOfCachedSingleCharStrings = 2048;

    public static bool TryGet(char c, [MaybeNullWhen(false)] out string str)
    {
        if (c >= AmountOfCachedSingleCharStrings)
        {
            return s_customSingleCharStringCache.TryGetValue(c, out str);
        }

        ReadOnlySpan<string> cachedSingleCharStrings = GetCachedSingleCharStrings();
        ref string reference = ref MemoryMarshal.GetReference(cachedSingleCharStrings);
        str = Unsafe.Add(ref reference, c);
        return true;
    }

    [Pure]
    public static string GetOrAdd(char c)
    {
        if (TryGet(c, out string? str))
        {
            return str;
        }

        str = char.ToString(c);
        s_customSingleCharStringCache.AddOrSet(c, str);
        return str;
    }

    public static void Add(string str)
    {
        if (str.Length != 1)
        {
            ThrowStringIsNotASingleCharString(str);
        }

        char c = StringMarshal.GetReference(str);
        if (c < AmountOfCachedSingleCharStrings)
        {
            return;
        }

        s_customSingleCharStringCache.AddOrSet(c, str);
    }

    [Pure]
    public static bool Contains(char c) => c < AmountOfCachedSingleCharStrings || s_customSingleCharStringCache.ContainsKey(c);

    internal static partial ReadOnlySpan<string> GetCachedSingleCharStrings();

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStringIsNotASingleCharString(string str)
        => throw new InvalidOperationException($"The provided string's (\"{str}\") length is not 1.");
}
