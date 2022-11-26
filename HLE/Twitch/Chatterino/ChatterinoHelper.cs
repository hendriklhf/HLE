using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;

namespace HLE.Twitch.Chatterino;

/// <summary>
/// A class to help with the application <a href="https://www.chatterino.com">Chatterino</a>.
/// </summary>
public static class ChatterinoHelper
{
    /// <summary>
    /// Gets all channels of all your tabs from the Chatterino settings.
    /// </summary>
    /// <returns>A <see cref="List{String}"/> of all channels.</returns>
    /// <exception cref="JsonException">Will be thrown if the JSON settings file is not of the expected format.</exception>
    [SupportedOSPlatform("windows")]
    public static string[] GetChannels()
    {
        const string dataProperty = "data";
        const string typeProperty = "type";
        const string twitchProperty = "twitch";
        const string nameProperty = "name";
        const string itemsProperty = "items";
        const string splits2Property = "splits2";

        static void GetChannelsFromSplits(JsonElement splits, List<string> channels)
        {
            if (splits.TryGetProperty(dataProperty, out JsonElement data))
            {
                if (data.GetProperty(typeProperty).GetString() != twitchProperty)
                {
                    return;
                }

                string channel = data.GetProperty(nameProperty).GetString()?.ToLower() ??
                    throw new JsonException($"Property \"{nameProperty}\" does not exist. This method might not work anymore due to changes in the Chatterino settings format.");
                if (!channels.Contains(channel))
                {
                    channels.Add(channel);
                }
            }
            else
            {
                JsonElement items = splits.GetProperty(itemsProperty);
                for (int i = 0; i < items.GetArrayLength(); i++)
                {
                    GetChannelsFromSplits(items[i], channels);
                }
            }
        }

        List<string> result = new();
        string windowLayoutPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Chatterino2\Settings\window-layout.json";
        JsonElement chatterinoTabs = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(windowLayoutPath)).GetProperty("windows")[0].GetProperty("tabs");
        for (int i = 0; i < chatterinoTabs.GetArrayLength(); i++)
        {
            JsonElement splits = chatterinoTabs[i].GetProperty(splits2Property);
            GetChannelsFromSplits(splits, result);
        }

        return result.ToArray();
    }
}
