using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CellManager.ViewModels;

namespace CellManager.Services
{
    public class ProtectionProfileLoader
    {
        private readonly string _profilesPath;

        public ProtectionProfileLoader()
        {
            _profilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProtectionProfiles");
        }

        public bool Exists(string version)
        {
            var filePath = Path.Combine(_profilesPath, $"{version}.json");
            return File.Exists(filePath);
        }

        public IEnumerable<ProtectionSetting>? Load(string version)
        {
            var filePath = Path.Combine(_profilesPath, $"{version}.json");
            if (!File.Exists(filePath))
            {
                return null;
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<ProtectionSetting>>(json);
        }

        public IEnumerable<string> GetAvailableVersions()
        {
            if (!Directory.Exists(_profilesPath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(_profilesPath, "*.json")
                .Select(Path.GetFileNameWithoutExtension);
        }
    }
}
