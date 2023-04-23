﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HLE.Twitch.Api.Models;

namespace HLE.Twitch.Api.JsonConverters;

internal sealed class BroadcasterTypeJsonConverter : JsonConverter<BroadcasterType>
{
    public override BroadcasterType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.ValueSpan.Length == 0)
        {
            return BroadcasterType.Normal;
        }

        return reader.ValueSpan[0] switch
        {
            (byte)'a' => BroadcasterType.Affiliate,
            (byte)'p' => BroadcasterType.Partner,
            _ => BroadcasterType.Normal
        };
    }

    public override void Write(Utf8JsonWriter writer, BroadcasterType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            BroadcasterType.Affiliate => "affiliate",
            BroadcasterType.Partner => "partner",
            _ => string.Empty
        });
    }
}
