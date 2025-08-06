using System;
using System.IO;
using Newtonsoft.Json;
using WpfDwmManager.Models;

namespace WpfDwmManager.Utils
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WpfDwmManager",
            "config.json");

        public static Configuration LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonConvert.DeserializeObject<Configuration>(json);
                    return config ?? CreateDefaultConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
            }

            return CreateDefaultConfiguration();
        }

        public static void SaveConfiguration(Configuration config)
        {
            try
            {
                string directory = Path.GetDirectoryName(ConfigPath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        private static Configuration CreateDefaultConfiguration()
        {
            var config = new Configuration();
            
            // Add default window filter rules
            config.WindowFilterRules.AddRange(new[]
            {
                "dwm.exe",
                "winlogon.exe",
                "csrss.exe",
                "explorer.exe",
                "taskmgr.exe",
                "WpfDwmManager.exe"
            });

            // Save default config
            SaveConfiguration(config);
            
            return config;
        }
    }
}