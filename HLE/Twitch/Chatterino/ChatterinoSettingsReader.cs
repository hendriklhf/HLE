using System;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    private readonly string _windowLayoutPath;
    private string[]? _channels;

    public ChatterinoSettingsReader()
    {
        using PooledStringBuilder pathBuilder = new(128);
        pathBuilder.Append(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\Chatterino2\Settings\window-layout.json");
        _windowLayoutPath = StringPool.Shared.GetOrAdd(pathBuilder.WrittenSpan);
    }

    /// <summary>
    /// Gets all distinct channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <remarks>The result will be cached, thus only the first call will allocate.</remarks>
    /// <returns>A string array of all channels.</returns>
    public ImmutableArray<string> GetChannels()
    {
        if (_channels is not null)
        {
            return ImmutableCollectionsMarshal.AsImmutableArray(_channels);
        }

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

        _channels = channels.ToArray();
        return ImmutableCollectionsMarshal.AsImmutableArray(_channels);
    }

    private void ReadWindowLayoutFile(PooledBufferWriter<byte> windowLayoutFileContentWriter)
    {
        BufferedFileReader fileReader = new(_windowLayoutPath);
        fileReader.ReadBytes(windowLayoutFileContentWriter);
    }

    [Pure]
    public bool Equals(ChatterinoSettingsReader? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ChatterinoSettingsReader? left, ChatterinoSettingsReader? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ChatterinoSettingsReader? left, ChatterinoSettingsReader? right)
    {
        return !(left == right);
    }
}
