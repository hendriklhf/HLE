using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;

namespace HLE.Twitch.Chatterino
{
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
        public static IEnumerable<string> GetChannels()
        {
            List<string> result = new();

            void GetChannelsFromSplits(JsonElement splits)
            {
                if (splits.TryGetProperty("data", out JsonElement data))
                {
                    if (data.GetProperty("type").GetString() != "twitch")
                    {
                        return;
                    }

                    string channel = data.GetProperty("name").GetString()?.ToLower() ??
                                     throw new JsonException("Property \"name\" does not exist. This method might not work anymore due to changes in the Chatterino settings format.");
                    if (!result.Contains(channel))
                    {
                        result.Add(channel);
                    }
                }
                else
                {
                    JsonElement items = splits.GetProperty("items");
                    for (int i = 0; i < items.GetArrayLength(); i++)
                    {
                        GetChannelsFromSplits(items[i]);
                    }
                }
            }

            string windowLayoutPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Chatterino2\Settings\window-layout.json";
            JsonElement chatterinoTabs = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(windowLayoutPath)).GetProperty("windows")[0].GetProperty("tabs");
            for (int i = 0; i < chatterinoTabs.GetArrayLength(); i++)
            {
                JsonElement splits = chatterinoTabs[i].GetProperty("splits2");
                GetChannelsFromSplits(splits);
            }

            return result;
        }
    }
}
