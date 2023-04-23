﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HLE.Twitch.Api.Models;

namespace HLE.Twitch.Api.JsonConverters;

public sealed class EmoteThemeJsonConverter : JsonConverter<EmoteThemes>
{
    public override EmoteThemes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        EmoteThemes result = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                continue;
            }

            result |= reader.ValueSpan[0] switch
            {
                (byte)'l' => EmoteThemes.Light,
                (byte)'d' => EmoteThemes.Dark,
                _ => 0
            };
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, EmoteThemes value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        if ((value & EmoteThemes.Light) == EmoteThemes.Light)
        {
            writer.WriteStringValue(Emote.ThemeValues[EmoteThemes.Light]);
        }

        if ((value & EmoteThemes.Dark) == EmoteThemes.Dark)
        {
            writer.WriteStringValue(Emote.ThemeValues[EmoteThemes.Dark]);
        }

        writer.WriteEndArray();
    }
}
