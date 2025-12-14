using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HLE.Text;

public sealed partial class LazyString
{
    internal sealed class JsonConverter : JsonConverter<LazyString?>,
        IEquatable<JsonConverter>
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

        public override void Write(Utf8JsonWriter writer, LazyString? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
                {
                    return;
                }

                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.AsSpan());
        }

        [Pure]
        public bool Equals([NotNullWhen(true)] JsonConverter? other) => ReferenceEquals(this, other);

        [Pure]
        public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

        [Pure]
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

        public static bool operator ==(JsonConverter? left, JsonConverter? right) => Equals(left, right);

        public static bool operator !=(JsonConverter? left, JsonConverter? right) => !(left == right);
    }
}
