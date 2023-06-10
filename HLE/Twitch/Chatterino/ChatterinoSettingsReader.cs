using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
public sealed class ChatterinoSettingsReader : IDisposable, IEquatable<ChatterinoSettingsReader>
{
    private readonly PoolBufferWriter<byte> _windowLayoutFileContentWriter = new(5000, 5000);
    private string[]? _channels;

    public ChatterinoSettingsReader()
    {
        using PoolBufferStringBuilder pathBuilder = new(100);
        pathBuilder.Append(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\Chatterino2\Settings\window-layout.json");
        string windowLayoutPath = StringPool.Shared.GetOrAdd(pathBuilder.WrittenSpan);
        Files.ReadBytes(windowLayoutPath, _windowLayoutFileContentWriter);
    }

    /// <summary>
    /// Gets all distinct channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A string array of all channels.</returns>
    [Pure]
    public string[] GetChannels()
    {
        if (_channels is not null)
        {
            return _channels;
        }

        Utf8JsonReader jsonReader = new(_windowLayoutFileContentWriter.WrittenSpan);
        using PoolBufferList<string> channels = new(20, 15);
        HashSet<int> channelHashes = new(20);
        Span<char> charBuffer = stackalloc char[30];

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

                    int channelLength = Encoding.UTF8.GetChars(channelNameAsBytes, charBuffer);
                    int channelHash = string.GetHashCode(charBuffer[..channelLength], StringComparison.OrdinalIgnoreCase);
                    if (channelHashes.Add(channelHash))
                    {
                        channels.Add(new(charBuffer[..channelLength]));
                    }

                    break;
                default:
                    continue;
            }
        }

        return _channels = channels.ToArray();
    }

    public void Dispose()
    {
        _windowLayoutFileContentWriter.Dispose();
    }

    public bool Equals(ChatterinoSettingsReader? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is ChatterinoSettingsReader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }

    public static bool operator ==(ChatterinoSettingsReader? left, ChatterinoSettingsReader? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ChatterinoSettingsReader? left, ChatterinoSettingsReader? right)
    {
        return !(left == right);
    }
}
