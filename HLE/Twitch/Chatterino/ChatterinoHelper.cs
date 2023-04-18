using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Twitch.Chatterino;

/// <summary>
/// A class to help with the application <a href="https://www.chatterino.com">Chatterino</a>.
/// </summary>
public static class ChatterinoHelper
{
    /// <summary>
    /// Gets all channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A string array of all channels.</returns>
    /// <exception cref="JsonException">Will be thrown if the JSON settings file is not of the expected format.</exception>
    [Pure]
    [SupportedOSPlatform("windows")]
    public static string[] GetChannels()
    {
        string windowLayoutPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Chatterino2\Settings\window-layout.json";
        using PoolBufferWriter<byte> fileContentWriter = new(5000, 5000);
        Files.ReadBytes(windowLayoutPath, fileContentWriter);

        Utf8JsonReader jsonReader = new(fileContentWriter.WrittenSpan);
        using PoolBufferList<string> channels = new(20, 15);
        HashSet<string> channelHashes = new(20);
        ReadOnlySpan<byte> dataProperty = "data"u8;
        ReadOnlySpan<byte> nameProperty = "name"u8;
        ReadOnlySpan<byte> typeProperty = "type"u8;
        ReadOnlySpan<byte> twitchTypeValue = "twitch"u8;
        while (jsonReader.Read())
        {
            switch (jsonReader.TokenType)
            {
                case JsonTokenType.PropertyName when jsonReader.ValueTextEquals(dataProperty):
                    jsonReader.Read();
                    jsonReader.Read();
                    if (!jsonReader.ValueTextEquals(nameProperty))
                    {
                        continue;
                    }

                    jsonReader.Read();
                    ReadOnlySpan<byte> channelNameAsBytes = jsonReader.ValueSpan;
                    jsonReader.Read();
                    if (!jsonReader.ValueTextEquals(typeProperty))
                    {
                        continue;
                    }

                    jsonReader.Read();
                    if (!jsonReader.ValueTextEquals(twitchTypeValue))
                    {
                        continue;
                    }

                    string channelName = Encoding.UTF8.GetString(channelNameAsBytes);
                    bool added = channelHashes.Add(channelName);
                    if (!added)
                    {
                        continue;
                    }

                    channels.Add(channelName);
                    break;
                default:
                    continue;
            }
        }

        return channels.ToArray();
    }
}
