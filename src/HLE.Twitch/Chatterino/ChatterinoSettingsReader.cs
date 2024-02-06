using System;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using HLE.Collections;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Twitch.Chatterino;

/// <summary>
/// Reads settings of the application <a href="https://www.chatterino.com">Chatterino</a>.
/// </summary>
[SupportedOSPlatform("windows")]
public static class ChatterinoSettingsReader
{
    private static readonly string s_windowLayoutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Chatterino2\Settings\window-layout.json";

    /// <summary>
    /// Gets all distinct channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A string array of all channels.</returns>
    public static string[] GetChannels()
    {
        using PooledBufferWriter<byte> windowLayoutFileContentWriter = new(32_000);
        ReadWindowLayoutFile(windowLayoutFileContentWriter);

        Utf8JsonReader jsonReader = new(windowLayoutFileContentWriter.WrittenSpan);
        using ValueList<string> channels = new(20);

        ReadOnlySpan<byte> dataProperty = "data"u8;
        ReadOnlySpan<byte> nameProperty = "name"u8;
        ReadOnlySpan<byte> typeProperty = "type"u8;
        ReadOnlySpan<byte> twitchTypeValue = "twitch"u8;

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals(dataProperty))
            {
                continue;
            }

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

            string channel = StringPool.Shared.GetOrAdd(channelNameAsBytes, Encoding.UTF8);
            if (!channels.AsSpan().Contains(channel))
            {
                channels.Add(channel);
            }
        }

        return channels.ToArray();
    }

    private static void ReadWindowLayoutFile(PooledBufferWriter<byte> windowLayoutFileContentWriter)
    {
        using BufferedFileReader fileReader = new(s_windowLayoutPath);
        fileReader.ReadBytes(windowLayoutFileContentWriter);
    }
}
