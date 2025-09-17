using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     Placeholder view model for the export tab; toggled off until the feature is implemented.
    /// </summary>
    public partial class DataExportViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Data Export";
        public string IconName { get; } = "Export";

        [ObservableProperty]
        private bool _isViewEnabled = false;
    }
}