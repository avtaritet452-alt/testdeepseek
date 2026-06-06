using System;
using System.IO;
using System.Text.Json;

namespace LSDLauncher
{
    public class Settings
    {
        public bool DiscordRPCEnabled { get; set; } = true;
        public bool RandomNicknameEachLaunch { get; set; } = false;
        public bool KeepFiles { get; set; } = false;
        public string LastNickname { get; set; } = "";

        private static string SettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public void Save()
        {
            try { File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
            catch { }
        }

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath)) ?? new Settings();
            }
            catch { }
            return new Settings();
        }
    }
}