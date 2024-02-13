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
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public static class ChatterinoSettingsReader
{
    private static string? s_windowLayoutPath;

    private const string WindowLayoutFile = "window-layout.json";

    /// <summary>
    /// Gets all distinct channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A string array of all channels.</returns>
    public static string[] GetChannels()
    {
        using PooledBufferWriter<byte> windowLayoutFileContentWriter = new(64_000);
        ReadWindowLayoutFile(windowLayoutFileContentWriter);

        Utf8JsonReader jsonReader = new(windowLayoutFileContentWriter.WrittenSpan);
        using ValueList<string> channels = new(20);

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("data"u8))
            {
                continue;
            }

            jsonReader.Read();
            jsonReader.Read();
            if (!jsonReader.ValueTextEquals("name"u8))
            {
                continue;
            }

            jsonReader.Read();
            ReadOnlySpan<byte> channelNameAsBytes = jsonReader.ValueSpan;
            jsonReader.Read();
            if (!jsonReader.ValueTextEquals("type"u8))
            {
                continue;
            }

            jsonReader.Read();
            if (!jsonReader.ValueTextEquals("twitch"u8))
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
        using BufferedFileReader fileReader = new(s_windowLayoutPath ??= GetWindowLayoutPath());
        fileReader.ReadBytes(windowLayoutFileContentWriter);
    }

    private static string GetWindowLayoutPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Chatterino2\Settings\" + WindowLayoutFile;
        }

        if (OperatingSystem.IsLinux())
        {
            return $"~/.local/share/chatterino/Settings/{WindowLayoutFile}";
        }

        if (OperatingSystem.IsMacOS())
        {
            return $"~/Library/Application Support/chatterino/Settings/{WindowLayoutFile}";
        }

        ThrowHelper.ThrowPlatformNotSupportedException();
        return null!;
    }
}
