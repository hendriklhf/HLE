using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HLE.Memory;

namespace HLE.Text;

internal sealed class LazyStringJsonConverter :
    JsonConverter<LazyString>,
    IEquatable<LazyStringJsonConverter>
{
    public override LazyString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        ReadOnlySpan<byte> jsonValue = reader.ValueSpan;
        int maxCharCount = Encoding.UTF8.GetMaxCharCount(jsonValue.Length);
        char[] chars = ArrayPool<char>.Shared.Rent(maxCharCount);
        int charCount = Encoding.UTF8.GetChars(jsonValue, chars);
        return new(chars, charCount);
    }

    public override void Write(Utf8JsonWriter writer, LazyString value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.AsSpan());

    [Pure]
    public bool Equals([NotNullWhen(true)] LazyStringJsonConverter? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(LazyStringJsonConverter? left, LazyStringJsonConverter? right) => Equals(left, right);

    public static bool operator !=(LazyStringJsonConverter? left, LazyStringJsonConverter? right) => !(left == right);
}
