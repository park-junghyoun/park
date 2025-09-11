using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CellManager.ViewModels
{
    public static class ProtectionProfileLoader
    {
        public static IList<ProtectionSetting> Load(string firmwareVersion)
        {
            if (string.IsNullOrWhiteSpace(firmwareVersion))
            {
                return new List<ProtectionSetting>();
            }

            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "ProtectionProfiles", $"{firmwareVersion}.json");
            if (!File.Exists(path))
            {
                return new List<ProtectionSetting>();
            }

            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var settings = JsonSerializer.Deserialize<List<ProtectionSetting>>(json, options) ?? new List<ProtectionSetting>();

            foreach (var setting in settings)
            {
                setting.Spec = setting.DefaultSpec;
            }

            return settings;
        }
    }
}
