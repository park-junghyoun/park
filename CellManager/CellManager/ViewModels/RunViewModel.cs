using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    public partial class RunViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Run";
        public string IconName { get; } = "Play";

        [ObservableProperty]
        private bool _isViewEnabled = true;
    }
}