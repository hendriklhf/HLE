using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
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
public sealed class ChatterinoSettingsReader : IEquatable<ChatterinoSettingsReader>
{
    private static readonly string s_windowLayoutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Chatterino2\Settings\window-layout.json";

    /// <summary>
    /// Gets all distinct channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A string array of all channels.</returns>
    public static string[] GetChannels()
    {
        using PooledBufferWriter<byte> windowLayoutFileContentWriter = new(20_000);
        ReadWindowLayoutFile(windowLayoutFileContentWriter);

        Utf8JsonReader jsonReader = new(windowLayoutFileContentWriter.WrittenSpan);
        using PooledList<string> channels = new(20);

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
            if (!channels.Contains(channel))
            {
                channels.Add(channel);
            }
        }

        return channels.ToArray();
    }

    private static void ReadWindowLayoutFile(PooledBufferWriter<byte> windowLayoutFileContentWriter)
    {
        BufferedFileReader fileReader = new(s_windowLayoutPath);
        fileReader.ReadBytes(windowLayoutFileContentWriter);
    }

    [Pure]
    public bool Equals(ChatterinoSettingsReader? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ChatterinoSettingsReader? left, ChatterinoSettingsReader? right) => Equals(left, right);

    public static bool operator !=(ChatterinoSettingsReader? left, ChatterinoSettingsReader? right) => !(left == right);
}
