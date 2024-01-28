using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using HLE.Collections;
using HLE.Numerics;
using HLE.Twitch.Ffz.Models;

namespace HLE.Twitch.Ffz;

internal readonly ref struct ResponseDeserializer(ReadOnlySpan<byte> response)
{
    private readonly ReadOnlySpan<byte> _response = response;

    public ImmutableArray<Emote> Deserialize()
    {
        ReadOnlySpan<byte> emotesProperty = "emoticons"u8;

        // ReSharper disable once NotDisposedResource
        ValueList<Emote> emotes = new(50);
        try
        {
            Utf8JsonReader reader = new(_response);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(emotesProperty))
                {
                    DeserializeEmotes(ref reader, ref emotes);
                    break;
                }
            }

            return ImmutableCollectionsMarshal.AsImmutableArray(emotes.ToArray());
        }
        finally
        {
            emotes.Dispose();
        }
    }

    private static void DeserializeEmotes(ref Utf8JsonReader reader, ref ValueList<Emote> emotes)
    {
        ReadOnlySpan<byte> emoteIdProperty = "id"u8;
        ReadOnlySpan<byte> emoteNameProperty = "name"u8;
        ReadOnlySpan<byte> emoteOwnerProperty = "owner"u8;

        int emoteId = 0;
        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            if (reader.ValueTextEquals(emoteOwnerProperty))
            {
                reader.Skip();
                continue;
            }

            if (reader.ValueTextEquals(emoteIdProperty))
            {
                reader.Read();
                emoteId = NumberHelpers.ParsePositiveNumber<int>(reader.ValueSpan);
                continue;
            }

            if (!reader.ValueTextEquals(emoteNameProperty))
            {
                continue;
            }

            reader.Read();
            ReadOnlySpan<byte> emoteName = reader.ValueSpan;
            Emote emote = new()
            {
                Id = emoteId,
                Name = Encoding.UTF8.GetString(emoteName)
            };

            emotes.Add(emote);
        }
    }
}
