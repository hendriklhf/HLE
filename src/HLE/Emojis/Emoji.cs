using System;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HLE.Strings;

namespace HLE.Emojis;

#pragma warning disable CA1708

/// <summary>
/// A class that contains (almost) all existing emojis.
/// </summary>
public static partial class Emoji
{
    // Members are generated by a source generator.
    // Emojis will be string constants, e.g.:
    // public const string Grinning = "😀";

    public static ImmutableArray<string> Emojis => s_emojis.Items;

    private static readonly FrozenSet<string> s_emojis = typeof(Emoji)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(static f => f.FieldType == typeof(string))
        .Select(static f => Unsafe.As<string>(f.GetValue(null))!)
        .ToFrozenSet();

    [Pure]
    public static bool IsEmoji(char c)
    {
        using NativeString str = new(new ReadOnlySpan<char>(in c));
        return IsEmoji(str.AsString());
    }

    [Pure]
    public static bool IsEmoji(string text) => s_emojis.Contains(text);

    [Pure]
    public static bool IsEmoji(ReadOnlySpan<char> text)
    {
        using NativeString str = new(text);
        return IsEmoji(str.AsString());
    }
}