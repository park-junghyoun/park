using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

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

        public ObservableCollection<BoardSettingData> BoardSettingsData { get; } = new()
        {
            new BoardSettingData { Parameter = "Firmware Version", Value = "1.0" },
            new BoardSettingData { Parameter = "Serial Number", Value = "123456" }
        };
    }

    public class BoardSettingData
    {
        public string Parameter { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

