using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    public partial class DisplayViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Display";
        public string IconName { get; } = "Monitor";

        [ObservableProperty]
        private bool _isViewEnabled = false;
    }
}