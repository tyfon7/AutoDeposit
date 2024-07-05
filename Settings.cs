using BepInEx.Configuration;
using System.Collections.Generic;

namespace AutoDeposit
{
    public class Settings
    {
        // Categories
        private const string GeneralSection = "1. Buttons";

        // General
        public static ConfigEntry<bool> EnableRig { get; set; }
        public static ConfigEntry<bool> EnablePockets { get; set; }
        public static ConfigEntry<bool> EnableBackpack { get; set; }
        public static ConfigEntry<bool> EnableSecureContainer { get; set; }
        public static ConfigEntry<bool> EnableTransfer { get; set; }

        public static void Init(ConfigFile config)
        {
            var configEntries = new List<ConfigEntryBase>();

            // Buttons
            configEntries.Add(EnableRig = config.Bind(
                GeneralSection,
                "Rig",
                true,
                new ConfigDescription(
                    "Whether to show the autodeposit button on your rig",
                    null,
                    new ConfigurationManagerAttributes { })));

            configEntries.Add(EnablePockets = config.Bind(
                GeneralSection,
                "Pockets",
                true,
                new ConfigDescription(
                    "Whether to show the autodeposit button on your pockets",
                    null,
                    new ConfigurationManagerAttributes { })));

            configEntries.Add(EnableBackpack = config.Bind(
                GeneralSection,
                "Backpack",
                true,
                new ConfigDescription(
                    "Whether to show the autodeposit button on your backpack",
                    null,
                    new ConfigurationManagerAttributes { })));

            configEntries.Add(EnableSecureContainer = config.Bind(
                GeneralSection,
                "Secure Container",
                true,
                new ConfigDescription(
                    "Whether to show the autodeposit button on your secure container",
                    null,
                    new ConfigurationManagerAttributes { })));

            configEntries.Add(EnableTransfer = config.Bind(
                GeneralSection,
                "Transfer Screen",
                true,
                new ConfigDescription(
                    "Whether to show the autodeposit button on the transfer screen",
                    null,
                    new ConfigurationManagerAttributes { })));

            RecalcOrder(configEntries);
        }

        private static void RecalcOrder(List<ConfigEntryBase> configEntries)
        {
            // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
            int settingOrder = configEntries.Count;
            foreach (var entry in configEntries)
            {
                if (entry.Description.Tags[0] is ConfigurationManagerAttributes attributes)
                {
                    attributes.Order = settingOrder;
                }

                settingOrder--;
            }
        }
    }
}
