using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     Utility for loading protection profile definitions from JSON files located next to the application.
    /// </summary>
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
            var profile = JsonSerializer.Deserialize<ProtectionProfile>(json, options);
            if (profile?.Settings == null)
            {
                return new List<ProtectionSetting>();
            }

            var settings = new List<ProtectionSetting>();
            foreach (var s in profile.Settings)
            {
                var setting = new ProtectionSetting
                {
                    Parameter = s.Parameter,
                    Spec = s.Spec,
                    Unit = s.Unit,
                    Description = s.Description,
                    Category = s.Category
                };
                foreach (var option in s.Options)
                {
                    setting.Options.Add(option);
                }
                settings.Add(setting);
            }

            return settings;
        }
    }
}
