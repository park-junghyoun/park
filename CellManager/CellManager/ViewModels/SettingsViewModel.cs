using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace CellManager.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Settings";
        public string IconName { get; } = "Cog";

        [ObservableProperty]
        private bool _isViewEnabled = true;

        // Board configuration
        [ObservableProperty]
        private string _boardPort = string.Empty;

        // Calibration placeholders
        [ObservableProperty]
        private string _voltageLowReference = "1000";

        [ObservableProperty]
        private string _voltageLowResult = "0";

        [ObservableProperty]
        private string _voltageHighReference = "4000";

        [ObservableProperty]
        private string _voltageHighResult = "0";

        [ObservableProperty]
        private string _currentReference = "500";

        [ObservableProperty]
        private string _currentResult = "0";

        [ObservableProperty]
        private string _temperatureReference = "25";

        [ObservableProperty]
        private string _temperatureResult = "0";

        // Default paths
        [ObservableProperty]
        private string _dataExportPath = string.Empty;

        [ObservableProperty]
        private string _profilePath = string.Empty;

        // Feature toggles
        [ObservableProperty]
        private bool _enableAdvancedFeatures;

        [ObservableProperty]
        private bool _enableDarkTheme;

        // Firmware
        [ObservableProperty]
        private string _firmwareVersion = string.Empty;

        public ObservableCollection<BoardSettingData> BoardSettingsData { get; } = new()
        {
            new BoardSettingData { Parameter = "Firmware Version", Value = "1.0" },
            new BoardSettingData { Parameter = "Serial Number", Value = "123456" }
        };

        public ObservableCollection<ProtectionSetting> ProtectionSettings { get; } = new();

        public RelayCommand ReadProtectionCommand { get; }
        public RelayCommand WriteProtectionCommand { get; }

        private readonly Dictionary<string, List<ProtectionSetting>> _profiles = new();

        public SettingsViewModel()
        {
            LoadProtectionConfigurations();
            ReadProtectionCommand = new RelayCommand(ReadProtectionSettings, CanReadWriteProtection);
            WriteProtectionCommand = new RelayCommand(WriteProtectionSettings, CanReadWriteProtection);
        }

        private IEnumerable<string> SupportedFirmwareVersions => _profiles.Keys;

        private bool CanReadWriteProtection() => _profiles.ContainsKey(FirmwareVersion);

        private void ReadProtectionSettings()
        {
            if (!CanReadWriteProtection())
            {
                MessageBox.Show($"Unsupported firmware version. Supported versions: {string.Join(", ", SupportedFirmwareVersions)}");
                return;
            }

            ProtectionSettings.Clear();
            foreach (var setting in _profiles[FirmwareVersion])
            {
                ProtectionSettings.Add(setting);
            }
        }

        private void WriteProtectionSettings()
        {
            if (!CanReadWriteProtection())
            {
                MessageBox.Show($"Unsupported firmware version. Supported versions: {string.Join(", ", SupportedFirmwareVersions)}");
                return;
            }

            // Placeholder for writing logic
        }

        partial void OnFirmwareVersionChanged(string value)
        {
            ReadProtectionCommand.NotifyCanExecuteChanged();
            WriteProtectionCommand.NotifyCanExecuteChanged();
        }
    }

    public class BoardSettingData
    {
        public string Parameter { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ProtectionSetting
    {
        public string Parameter { get; set; } = string.Empty;
        public string Spec { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ObservableCollection<string> Options { get; } = new();
    }

    internal class ProtectionProfile
    {
        [JsonPropertyName("version")] public string Version { get; set; } = string.Empty;
        [JsonPropertyName("settings")] public List<ProtectionSettingConfig> Settings { get; set; } = new();
    }

    internal class ProtectionSettingConfig
    {
        [JsonPropertyName("parameter")] public string Parameter { get; set; } = string.Empty;
        [JsonPropertyName("unit")] public string Unit { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("defaultSpec")] public string Spec { get; set; } = string.Empty;
        [JsonPropertyName("options")] public List<string> Options { get; set; } = new();
    }

    partial class SettingsViewModel
    {
        private void LoadProtectionConfigurations()
        {
            var profilesPath = Path.Combine(AppContext.BaseDirectory, "ProtectionProfiles");
            if (!Directory.Exists(profilesPath))
                return;

            foreach (var file in Directory.GetFiles(profilesPath, "*.json"))
            {
                try
                {
                    var profile = JsonSerializer.Deserialize<ProtectionProfile>(File.ReadAllText(file));
                    if (profile?.Version == null)
                        continue;

                    var settings = new List<ProtectionSetting>();
                    foreach (var s in profile.Settings)
                    {
                        var ps = new ProtectionSetting
                        {
                            Parameter = s.Parameter,
                            Spec = s.Spec,
                            Unit = s.Unit,
                            Description = s.Description
                        };
                        foreach (var option in s.Options)
                        {
                            ps.Options.Add(option);
                        }
                        settings.Add(ps);
                    }

                    _profiles[profile.Version] = settings;
                }
                catch
                {
                    // Ignore invalid profiles
                }
            }
        }
    }
}

