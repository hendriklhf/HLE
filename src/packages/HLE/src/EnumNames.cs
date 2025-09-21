using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE;

public static class EnumNames<TEnum> where TEnum : struct, Enum
{
    private static readonly string[] s_values = Enum.GetNames<TEnum>();

    public static int Count => s_values.Length;

    [Pure]
    public static ReadOnlySpan<string> AsSpan() => s_values;

    [Pure]
    public static ReadOnlyMemory<string> AsMemory() => s_values;

    [Pure]
    public static ImmutableArray<string> AsImmutableArray() => ImmutableCollectionsMarshal.AsImmutableArray(s_values);

    [Pure]
    public static IEnumerable<string> AsEnumerable() => s_values;
}
