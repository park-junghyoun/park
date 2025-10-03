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
    /// <summary>
    ///     Hosts firmware- and board-related configuration, including profile loading from JSON assets.
    /// </summary>
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
        private string _voltageLowReference = "2000";

        [ObservableProperty]
        private string _voltageLowResult = "Wait";

        [ObservableProperty]
        private string _voltageHighReference = "4000";

        [ObservableProperty]
        private string _voltageHighResult = "Wait";

        [ObservableProperty]
        private string _packVoltageLowReference = "2000";

        [ObservableProperty]
        private string _packVoltageLowResult = "Wait";

        [ObservableProperty]
        private string _packVoltageHighReference = "4000";

        [ObservableProperty]
        private string _packVoltageHighResult = "Wait";

        [ObservableProperty]
        private string _voltageAdcValue = "0";

        [ObservableProperty]
        private string _packVoltageAdcValue = "0";

        [ObservableProperty]
        private string _voltageMeasurement = "0";

        [ObservableProperty]
        private string _packVoltageMeasurement = "0";

        [ObservableProperty]
        private string _currentReference = "1000";

        [ObservableProperty]
        private string _currentResult = "Wait";

        [ObservableProperty]
        private string _curr0AReference = "0";

        [ObservableProperty]
        private string _curr0AResult = "Wait";

        [ObservableProperty]
        private string _currentAdcValue = "0";

        [ObservableProperty]
        private string _currentMeasurement = "0";

        [ObservableProperty]
        private string _temperatureReference = "25";

        [ObservableProperty]
        private string _temperatureResult = "Wait";

        [ObservableProperty]
        private string _temperatureAdcValue = "0";

        [ObservableProperty]
        private string _temperatureMeasurement = "0";

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

        [ObservableProperty]
        private bool _enableNotifications;

        [ObservableProperty]
        private bool _enableAutoUpdates;

        // Firmware
        [ObservableProperty]
        private string _firmwareVersion = "1.0";

        [ObservableProperty]
        private string _firmwareSubVersion = string.Empty;

        [ObservableProperty]
        private string _firmwareChecksum = string.Empty;

        public ObservableCollection<BoardDataSetting> BoardDataSettings { get; } = new();

        public ObservableCollection<ProtectionSetting> ProtectionSettings { get; } = new();

        public RelayCommand ReadProtectionCommand { get; }
        public RelayCommand WriteProtectionCommand { get; }
        public RelayCommand ImportProtectionCommand { get; }
        public RelayCommand ExportProtectionCommand { get; }
        public RelayCommand ReadBoardDataCommand { get; }
        public RelayCommand WriteBoardDataCommand { get; }
        public RelayCommand ImportBoardDataCommand { get; }
        public RelayCommand ExportBoardDataCommand { get; }
        public RelayCommand ReadAdcValuesCommand { get; }

        private readonly Dictionary<string, List<ProtectionSetting>> _profiles = new();
        private readonly string _boardDataFilePath = Path.Combine(AppContext.BaseDirectory, "BoardDataProfiles", "1.0.json");

        public SettingsViewModel()
        {
            LoadProtectionConfigurations();
            ReadProtectionCommand = new RelayCommand(ReadProtectionSettings, CanReadWriteProtection);
            WriteProtectionCommand = new RelayCommand(WriteProtectionSettings, CanReadWriteProtection);
            ReadBoardDataCommand = new RelayCommand(ReadBoardDataSettings);
            WriteBoardDataCommand = new RelayCommand(WriteBoardDataSettings);
            ImportBoardDataCommand = new RelayCommand(ImportBoardDataSettings);
            ExportBoardDataCommand = new RelayCommand(ExportBoardDataSettings);
            ImportProtectionCommand = new RelayCommand(ImportProtectionSettings, CanReadWriteProtection);
            ExportProtectionCommand = new RelayCommand(ExportProtectionSettings, CanReadWriteProtection);
            ReadAdcValuesCommand = new RelayCommand(ReadAdcValues);
            ReadProtectionSettings();
            ReadBoardDataSettings();
        }

        private IEnumerable<string> SupportedFirmwareVersions => _profiles.Keys;

        private bool CanReadWriteProtection() => _profiles.ContainsKey(FirmwareVersion);

        /// <summary>Loads protection settings for the currently selected firmware version.</summary>
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

        /// <summary>Placeholder for pushing updated protection settings back to hardware.</summary>
        private void WriteProtectionSettings()
        {
            if (!CanReadWriteProtection())
            {
                MessageBox.Show($"Unsupported firmware version. Supported versions: {string.Join(", ", SupportedFirmwareVersions)}");
                return;
            }

            // Placeholder for writing logic
        }

        private void ImportProtectionSettings()
        {
            if (!CanReadWriteProtection())
            {
                MessageBox.Show($"Unsupported firmware version. Supported versions: {string.Join(", ", SupportedFirmwareVersions)}");
                return;
            }

            // Placeholder for import logic
        }

        private void ExportProtectionSettings()
        {
            if (!CanReadWriteProtection())
            {
                MessageBox.Show($"Unsupported firmware version. Supported versions: {string.Join(", ", SupportedFirmwareVersions)}");
                return;
            }

            // Placeholder for export logic
        }

        private void ReadAdcValues()
        {
            // Placeholder for reading ADC values from hardware
        }

        private void ImportBoardDataSettings()
        {
            // Placeholder for import logic
        }

        private void ExportBoardDataSettings()
        {
            // Placeholder for export logic
        }

        partial void OnFirmwareVersionChanged(string value)
        {
            ReadProtectionCommand.NotifyCanExecuteChanged();
            WriteProtectionCommand.NotifyCanExecuteChanged();
            ImportProtectionCommand.NotifyCanExecuteChanged();
            ExportProtectionCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>Editable board configuration entry loaded from JSON metadata.</summary>
    public class BoardDataSetting
    {
        public string Parameter { get; set; } = string.Empty;
        public string Spec { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public ObservableCollection<string> Options { get; } = new();
    }

    /// <summary>Represents a single protection threshold or delay exposed to the UI.</summary>
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
        /// <summary>Reads protection configuration templates from disk into memory.</summary>
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

        /// <summary>Loads board data settings from the JSON profile file.</summary>
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

        /// <summary>Persists the in-memory board data settings back to disk.</summary>
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

