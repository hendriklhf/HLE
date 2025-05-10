using System;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Text;

public sealed partial class LazyString
{
    public static string operator +(LazyString? left, LazyString? right) => Add(left, right);

    public static string operator +(LazyString? left, string? right) => Add(left, right);

    public static string operator +(string? left, LazyString? right) => Add(left, right);

    public static string operator +(LazyString? left, ReadOnlySpan<char> right) => Add(left, right);

    public static string operator +(ReadOnlySpan<char> left, LazyString? right) => Add(left, right);

    public static string Add(LazyString? left, LazyString? right) => string.Concat((left ?? Empty).AsSpan(), (right ?? Empty).AsSpan());

    public static string Add(LazyString? left, string? right) => string.Concat((left ?? Empty).AsSpan(), right);

    public static string Add(string? left, LazyString? right) => string.Concat(left.AsSpan(), (right ?? Empty).AsSpan());

    public static string Add(LazyString? left, ReadOnlySpan<char> right) => string.Concat((left ?? Empty).AsSpan(), right);

    public static string Add(ReadOnlySpan<char> left, LazyString? right) => string.Concat(left, (right ?? Empty).AsSpan());
}
