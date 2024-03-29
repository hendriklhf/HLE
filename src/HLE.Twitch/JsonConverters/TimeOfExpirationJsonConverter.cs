﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HLE.Numerics;
using HLE.Twitch.Helix.Models;

namespace HLE.Twitch.JsonConverters;

internal sealed class TimeOfExpirationJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int expiresInSeconds = NumberHelpers.ParsePositiveNumber<int>(reader.ValueSpan);
        TimeSpan expiresIn = TimeSpan.FromMilliseconds(expiresInSeconds);
        return DateTime.UtcNow + expiresIn;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => throw new NotSupportedException($"The property {nameof(AccessToken)}.{nameof(AccessToken.TimeOfExpiration)} is not available for serialization.");
}
