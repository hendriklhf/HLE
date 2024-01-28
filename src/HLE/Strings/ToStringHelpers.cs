using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public static string FormatCollection<TCollection, TElement>(TCollection collection) where TCollection : ICollection<TElement>
        => FormatCollection(typeof(TCollection), collection.Count);

    [Pure]
    public static string FormatCollection<TCollection>(int elementCount)
        => FormatCollection(typeof(TCollection), elementCount);

    [Pure]
    [SkipLocalsInit]
    public static string FormatCollection(Type collectionType, int elementCount)
    {
        // ReSharper disable once NotDisposedResource
        ValueStringBuilder builder = new(stackalloc char[256]);
        try
        {
            AppendTypeAndGenericParameters(collectionType, ref builder);

            if (builder.WrittenSpan.EndsWith("[]"))
            {
                builder.Advance(-2);
            }

            builder.Append('[');
            builder.Append(elementCount);
            builder.Append(']');

            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static void AppendTypeAndGenericParameters(Type type, ref ValueStringBuilder builder)
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
        AppendTypeAndGenericParameters(genericArgumentsReference, ref builder);

        for (int i = 1; i < genericArguments.Length; i++)
        {
            builder.Append(", ");
            AppendTypeAndGenericParameters(Unsafe.Add(ref genericArgumentsReference, i), ref builder);
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
