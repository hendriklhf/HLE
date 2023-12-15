using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Strings;

internal static class ToStringHelpers
{
    private static readonly ConcurrentDictionary<Type, Type[]> s_genericTypeArgumentsCache = new();

    [Pure]
    public static string FormatCollection<TCountable>(TCountable countable) where TCountable : ICountable
        => FormatCollection(typeof(TCountable), countable.Count);

    [Pure]
    public static string FormatCollection<TCollection>(int elementCount)
        => FormatCollection(typeof(TCollection), elementCount);

    [Pure]
    public static string FormatCollection(Type collectionType, int elementCount)
    {
        using PooledStringBuilder builder = new(32);

        AppendTypeAndGenericParameters(collectionType, builder);

        builder.Append('[');
        builder.Append(elementCount);
        builder.Append(']');

        return builder.ToString();
    }

    private static void AppendTypeAndGenericParameters(Type type, PooledStringBuilder builder)
    {
        builder.Append(type.Namespace);
        builder.Append('.');
        builder.Append(FormatTypeName(type.Name));

        if (!type.IsGenericType)
        {
            return;
        }

        builder.Append('<');

        ReadOnlySpan<Type> genericArguments = GetGenericTypeArguments(type);
        ref Type genericArgumentsReference = ref MemoryMarshal.GetReference(genericArguments);
        AppendTypeAndGenericParameters(genericArgumentsReference, builder);

        for (int i = 1; i < genericArguments.Length; i++)
        {
            builder.Append(", ");
            AppendTypeAndGenericParameters(Unsafe.Add(ref genericArgumentsReference, i), builder);
        }

        builder.Append('>');
    }

    private static ReadOnlySpan<char> FormatTypeName(ReadOnlySpan<char> typeName)
    {
        int indexOfBacktick = typeName.LastIndexOf('`');
        return indexOfBacktick >= 0 ? typeName[..indexOfBacktick] : typeName;
    }

    private static ReadOnlySpan<Type> GetGenericTypeArguments(Type type)
    {
        Debug.Assert(type.IsGenericType);

        if (s_genericTypeArgumentsCache.TryGetValue(type, out Type[]? genericTypeArguments))
        {
            return genericTypeArguments;
        }

        genericTypeArguments = type.GenericTypeArguments;
        s_genericTypeArgumentsCache.AddOrSet(type, genericTypeArguments);
        return genericTypeArguments;
    }
}
