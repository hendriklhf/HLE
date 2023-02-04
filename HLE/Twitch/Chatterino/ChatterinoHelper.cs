using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;

namespace HLE.Twitch.Chatterino;

/// <summary>
/// A class to help with the application <a href="https://www.chatterino.com">Chatterino</a>.
/// </summary>
public static class ChatterinoHelper
{
    private const string _dataProperty = "data";
    private const string _typeProperty = "type";
    private const string _twitchProperty = "twitch";
    private const string _nameProperty = "name";
    private const string _itemsProperty = "items";
    private const string _splits2Property = "splits2";

    /// <summary>
    /// Gets all channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A string array of all channels.</returns>
    /// <exception cref="JsonException">Will be thrown if the JSON settings file is not of the expected format.</exception>
    [SupportedOSPlatform("windows")]
    public static string[] GetChannels()
    {
        static void GetChannelsFromSplits(JsonElement splits, List<string> channels, Span<string> channelsSpan)
        {
            if (splits.TryGetProperty(_dataProperty, out JsonElement data))
            {
                if (data.GetProperty(_typeProperty).GetString() != _twitchProperty)
                {
                    return;
                }

                string channel = data.GetProperty(_nameProperty).GetString()?.ToLower() ?? throw new JsonException($"Property \"{_nameProperty}\" does not exist. This method might not work anymore due to changes in the Chatterino settings format.");
                if (!channelsSpan.Contains(channel))
                {
                    channels.Add(channel);
                }
            }
            else
            {
                JsonElement items = splits.GetProperty(_itemsProperty);
                int itemsLength = items.GetArrayLength();
                for (int i = 0; i < itemsLength; i++)
                {
                    GetChannelsFromSplits(items[i], channels, channelsSpan);
                }
            }
        }

        List<string> result = new();
        Span<string> resultSpan = CollectionsMarshal.AsSpan(result);
        string windowLayoutPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Chatterino2\Settings\window-layout.json";
        JsonElement chatterinoTabs = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(windowLayoutPath)).GetProperty("windows")[0].GetProperty("tabs");
        int chatterinoTabsLength = chatterinoTabs.GetArrayLength();
        for (int i = 0; i < chatterinoTabsLength; i++)
        {
            JsonElement splits = chatterinoTabs[i].GetProperty(_splits2Property);
            GetChannelsFromSplits(splits, result, resultSpan);
        }

        return result.Distinct().ToArray();
    }
}
