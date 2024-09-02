using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using HLE.Collections;

namespace HLE.Text;

[SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "doesn't work because of constraints")]
internal static class ToStringHelpers
{
    private static readonly TypeFormatter s_formatter = new(new()
    {
        NamespaceSeparator = '.',
        GenericTypesSeparator = ", ",
        GenericDelimiters = new("<", ">")
    });

    [Pure]
    public static string FormatCollection<TCountable>(TCountable countable) where TCountable : ICountable, allows ref struct
        => FormatCollection(typeof(TCountable), countable.Count);

    [Pure]
    public static string FormatCollection<TCollection, TElement>(TCollection collection) where TCollection : ICollection<TElement>, allows ref struct
        => FormatCollection(typeof(TCollection), collection.Count);

    [Pure]
    public static string FormatCollection<TCollection>(int elementCount) where TCollection : allows ref struct
        => FormatCollection(typeof(TCollection), elementCount);

    [Pure]
    public static string FormatCollection(Type collectionType, int elementCount)
    {
        ReadOnlySpan<char> formattedType = s_formatter.Format(collectionType);
        if (collectionType.IsSZArray)
        {
            formattedType = formattedType[..^2];
        }

        return $"{formattedType}[{elementCount}]";
    }
}
