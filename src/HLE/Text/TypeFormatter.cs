using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Memory;

namespace HLE.Text;

public sealed class TypeFormatter(TypeFormattingOptions options) : IEquatable<TypeFormatter>
{
    private readonly TypeFormattingOptions _options = options;
    private readonly ConcurrentDictionary<Type, string> _cache = new();

    [Pure]
    public string Format<T>() => Format(typeof(T));

    [Pure]
    public string Format(Type type) => _cache.TryGetValue(type, out string? str) ? str : FormatCore(type);

    [SkipLocalsInit]
    private string FormatCore(Type type)
    {
        // ReSharper disable once NotDisposedResource
        ValueStringBuilder builder = new(stackalloc char[512]);
        try
        {
            AppendTypeAndGenericParameters(type, ref builder, true, true);
            string str = builder.ToString();
            _cache.TryAdd(type, str);
            return str;
        }
        finally
        {
            builder.Dispose();
        }
    }

    private void AppendTypeAndGenericParameters(Type type, ref ValueStringBuilder builder, [ConstantExpected] bool appendNamespace, [ConstantExpected] bool replaceNamespaceSeparators)
    {
        if (type.IsArray)
        {
            AppendArrayType(type, ref builder, replaceNamespaceSeparators);
            return;
        }

        if (appendNamespace)
        {
            AppendNamespace(type, ref builder, replaceNamespaceSeparators);
        }

        if (type.IsNested)
        {
            AppendTypeAndGenericParameters(type.DeclaringType!, ref builder, false, replaceNamespaceSeparators);
            builder.Append('.');
        }

        builder.Append(FormatTypeName(type.Name));

        ReadOnlySpan<Type> genericArguments = type.GenericTypeArguments;
        if (genericArguments.Length == 0)
        {
            return;
        }

        TypeFormattingOptions options = _options;
        builder.Append(options.GenericDelimiters.Opening);
        AppendTypeAndGenericParameters(genericArguments[0], ref builder, true, false);

        for (int i = 1; i < genericArguments.Length; i++)
        {
            builder.Append(options.GenericTypesSeparator);
            AppendTypeAndGenericParameters(genericArguments[i], ref builder, true, false);
        }

        builder.Append(options.GenericDelimiters.Closing);
    }

    private void AppendNamespace(Type type, ref ValueStringBuilder builder, bool replaceNamespaceSeparators)
    {
        ReadOnlySpan<char> typeNamespace = type.Namespace;
        FormatNamespace(ref builder, typeNamespace, replaceNamespaceSeparators);
        builder.Append(replaceNamespaceSeparators ? _options.NamespaceSeparator : '.');
    }

    private void AppendArrayType(Type type, ref ValueStringBuilder builder, [ConstantExpected] bool replaceNamespaceSeparators)
    {
        AppendTypeAndGenericParameters(type.GetElementType()!, ref builder, true, replaceNamespaceSeparators);
        builder.Append('[');
        builder.Append(',', type.GetArrayRank() - 1);
        builder.Append(']');
    }

    private static ReadOnlySpan<char> FormatTypeName(ReadOnlySpan<char> typeName)
    {
        int indexOfBacktick = typeName.LastIndexOf('`');
        return indexOfBacktick >= 0 ? typeName[..indexOfBacktick] : typeName;
    }

    private void FormatNamespace(ref ValueStringBuilder builder, ReadOnlySpan<char> typeNamespace, bool replaceNamespaceSeparators)
    {
        if (typeNamespace.Length == 0)
        {
            return;
        }

        if (!MemoryHelpers.UseStackalloc<char>(typeNamespace.Length))
        {
            using RentedArray<char> rentedBuffer = ArrayPool<char>.Shared.RentAsRentedArray(typeNamespace.Length);
            SpanHelpers.Copy(typeNamespace, rentedBuffer.AsSpan());
            builder.Append(rentedBuffer.AsSpan(..typeNamespace.Length));
        }
        else
        {
            Span<char> buffer = stackalloc char[typeNamespace.Length];
            SpanHelpers.Copy(typeNamespace, buffer);
            builder.Append(buffer[..typeNamespace.Length]);
        }

        if (replaceNamespaceSeparators)
        {
            builder[^typeNamespace.Length..].Replace('.', _options.NamespaceSeparator);
        }
    }

    public bool Equals(TypeFormatter? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(TypeFormatter? left, TypeFormatter? right) => Equals(left, right);

    public static bool operator !=(TypeFormatter? left, TypeFormatter? right) => !Equals(left, right);
}
