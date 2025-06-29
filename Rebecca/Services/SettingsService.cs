using Rebecca.Models;
using System.IO;
using StdEx.Serialization;

namespace Rebecca.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "Rebecca");
            Directory.CreateDirectory(appFolder); // Ensure the directory exists
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
        }

        public List<HotkeyConfig> LoadHotkeys()
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                return JsonUtils.Deserialize<List<HotkeyConfig>>(json) ?? new List<HotkeyConfig>();
            }
            return new List<HotkeyConfig>();
        }

        public void SaveHotkeys(List<HotkeyConfig> hotkeys)
        {
            string json = JsonUtils.Serialize(hotkeys, true);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
