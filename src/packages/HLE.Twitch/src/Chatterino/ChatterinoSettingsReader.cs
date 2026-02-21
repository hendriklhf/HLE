using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using HLE.Collections;
using HLE.Memory;
using HLE.Text;
#if NET10_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace HLE.Twitch.Chatterino;

/// <summary>
/// Reads settings of the application <a href="https://www.chatterino.com">Chatterino</a>.
/// </summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public static class ChatterinoSettingsReader
{
    private static readonly string s_windowLayoutPath = GetWindowLayoutPath();

    private const string WindowLayoutFileName = "window-layout.json";

    /// <summary>
    /// Gets all distinct channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A string array of all channels.</returns>
    public static string[] GetChannels()
    {
        ValueBufferWriter<byte> windowLayoutFileContentWriter = new(short.MaxValue);
        ReadWindowLayoutFile(ref windowLayoutFileContentWriter);

        Utf8JsonReader jsonReader = new(windowLayoutFileContentWriter.WrittenSpan);

#if NET10_0_OR_GREATER
        InlineArray16<string> buffer = default;
        using ValueList<string> channels = new(buffer);
#else
        using ValueList<string> channels = new(16);
#endif

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

        windowLayoutFileContentWriter.Dispose();
        return channels.ToArray();
    }

    private static void ReadWindowLayoutFile(ref ValueBufferWriter<byte> bufferWriter)
    {
        using FileStream fileStream = File.OpenRead(s_windowLayoutPath);
        long fileSize = fileStream.Length;
        if (fileSize == 0)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(fileSize, Array.MaxLength);

        int fileSize32 = (int)fileSize;
        Span<byte> buffer = bufferWriter.GetSpan(fileSize32).SliceUnsafe(..fileSize32);
        fileStream.ReadExactly(buffer);
        bufferWriter.Advance(fileSize32);
    }

    private static string GetWindowLayoutPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Chatterino2\Settings\" + WindowLayoutFileName;
        }

        if (OperatingSystem.IsLinux())
        {
            return $"~/.local/share/chatterino/Settings/{WindowLayoutFileName}";
        }

        if (OperatingSystem.IsMacOS())
        {
            return $"~/Library/Application Support/chatterino/Settings/{WindowLayoutFileName}";
        }

        ThrowHelper.ThrowOperatingSystemNotSupported();
        return null;
    }
}
