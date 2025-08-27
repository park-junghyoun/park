using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    public partial class DataExportViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Data Export";
        public string IconName { get; } = "Export";

        [ObservableProperty]
        private bool _isViewEnabled = false;
    }
}