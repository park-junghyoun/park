using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using CellManager.Messages;

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
        private string _firmwareVersion = "1.0";

        public ObservableCollection<BoardDataSetting> BoardDataSettings { get; } = new();

        public ObservableCollection<ProtectionSetting> ProtectionSettings { get; } = new();

        public RelayCommand ReadProtectionCommand { get; }
        public RelayCommand WriteProtectionCommand { get; }
        public RelayCommand ReadBoardDataCommand { get; }
        public RelayCommand WriteBoardDataCommand { get; }

        private readonly Dictionary<string, List<ProtectionSetting>> _profiles = new();
        private readonly string _boardDataFilePath = Path.Combine(AppContext.BaseDirectory, "BoardDataProfiles", "boarddata.json");

        public SettingsViewModel()
        {
            LoadProtectionConfigurations();
            ReadProtectionCommand = new RelayCommand(ReadProtectionSettings, CanReadWriteProtection);
            WriteProtectionCommand = new RelayCommand(WriteProtectionSettings, CanReadWriteProtection);
            ReadBoardDataCommand = new RelayCommand(ReadBoardDataSettings);
            WriteBoardDataCommand = new RelayCommand(WriteBoardDataSettings);
            ReadBoardDataSettings();
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
            WeakReferenceMessenger.Default.Send(new BoardVersionChangedMessage(value));
        }
    }

    public class BoardDataSetting
    {
        public string Parameter { get; set; } = string.Empty;
        public string Spec { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public ObservableCollection<string> Options { get; } = new();
    }

    public class ProtectionSetting
    {
        public string Parameter { get; set; } = string.Empty;
        public string Spec { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public ObservableCollection<string> Options { get; } = new();
    }

    internal class BoardDataProfile
    {
        [JsonPropertyName("settings")] public List<BoardDataSettingConfig> Settings { get; set; } = new();
    }

    internal class BoardDataSettingConfig
    {
        [JsonPropertyName("parameter")] public string Parameter { get; set; } = string.Empty;
        [JsonPropertyName("unit")] public string Unit { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("defaultSpec")] public string Spec { get; set; } = string.Empty;
        [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
        [JsonPropertyName("options")] public List<string> Options { get; set; } = new();
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
        [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
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
                            Description = s.Description,
                            Category = s.Category
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

        private void ReadBoardDataSettings()
        {
            try
            {
                if (!File.Exists(_boardDataFilePath))
                    return;

                var profile = JsonSerializer.Deserialize<BoardDataProfile>(File.ReadAllText(_boardDataFilePath));
                if (profile == null)
                    return;

                BoardDataSettings.Clear();
                foreach (var s in profile.Settings)
                {
                    var bd = new BoardDataSetting
                    {
                        Parameter = s.Parameter,
                        Spec = s.Spec,
                        Unit = s.Unit,
                        Description = s.Description,
                        Category = s.Category
                    };
                    foreach (var option in s.Options)
                    {
                        bd.Options.Add(option);
                    }
                    BoardDataSettings.Add(bd);
                }
            }
            catch
            {
                // Ignore invalid profiles
            }
        }

        private void WriteBoardDataSettings()
        {
            try
            {
                var profile = new BoardDataProfile
                {
                    Settings = BoardDataSettings.Select(b => new BoardDataSettingConfig
                    {
                        Parameter = b.Parameter,
                        Unit = b.Unit,
                        Description = b.Description,
                        Spec = b.Spec,
                        Category = b.Category,
                        Options = b.Options.ToList()
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
                var dir = Path.GetDirectoryName(_boardDataFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(_boardDataFilePath, json);
            }
            catch
            {
                // Ignore write errors
            }
        }
    }
}

