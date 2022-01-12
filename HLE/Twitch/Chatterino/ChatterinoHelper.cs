using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static IEnumerable<string> GetChannelsFromChatterinoSettings()
        {
            List<string> result = new();
            try
            {
                string chatterinoSettingsDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Chatterino2\Settings\window-layout.json";
                JsonElement chatterinoTabs = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(chatterinoSettingsDirectory)).GetProperty("windows")[0].GetProperty("tabs");
                for (int i = 0; i < chatterinoTabs.GetArrayLength(); i++)
                {
                    JsonElement tabSettings = chatterinoTabs[i].GetProperty("splits2");
                    try
                    {
                        if (tabSettings.GetProperty("data").GetProperty("type").GetString() == "twitch")
                        {
                            string? name = tabSettings.GetProperty("data").GetProperty("name").GetString();
                            if (name is not null)
                            {
                                result.Add(name);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            JsonElement tabItems = tabSettings.GetProperty("items");
                            for (int j = 0; j < tabItems.GetArrayLength(); j++)
                            {
                                if (tabItems[j].GetProperty("data").GetProperty("type").GetString() == "twitch")
                                {
                                    string? name = tabItems[j].GetProperty("data").GetProperty("name").GetString();
                                    if (name is not null)
                                    {
                                        result.Add(name);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return result.Distinct();
        }
    }
}
