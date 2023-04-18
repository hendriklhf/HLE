using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HLE.Twitch.Api.JsonConverters;

internal sealed class Int64StringConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return NumberHelper.ParsePositiveInt64(reader.ValueSpan);
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        Span<char> charBuffer = stackalloc char[20];
        value.TryFormat(charBuffer, out int charsWritten);
        writer.WriteString("id"u8, charBuffer[..charsWritten]);
    }
}
