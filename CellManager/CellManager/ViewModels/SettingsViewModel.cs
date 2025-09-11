using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

        private const string SupportedFirmwareVersion = "1.0";

        public SettingsViewModel()
        {
            ReadProtectionCommand = new RelayCommand(ReadProtectionSettings, CanReadWriteProtection);
            WriteProtectionCommand = new RelayCommand(WriteProtectionSettings, CanReadWriteProtection);
        }

        private bool CanReadWriteProtection() => FirmwareVersion == SupportedFirmwareVersion;

        private void ReadProtectionSettings()
        {
            if (!CanReadWriteProtection())
            {
                MessageBox.Show("Unsupported firmware version.");
                return;
            }

            ProtectionSettings.Clear();
            ProtectionSettings.Add(new ProtectionSetting
            {
                Parameter = "Charge Over Temperature",
                Unit = "°C",
                Spec = "55",
                Options = new() { "50", "55", "60" }
            });
            ProtectionSettings.Add(new ProtectionSetting { Parameter = "Over Voltage", Unit = "mV", Spec = "4200" });
            ProtectionSettings.Add(new ProtectionSetting { Parameter = "Over Current", Unit = "mA", Spec = "2000" });
        }

        private void WriteProtectionSettings()
        {
            if (!CanReadWriteProtection())
            {
                MessageBox.Show("Unsupported firmware version.");
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
}

