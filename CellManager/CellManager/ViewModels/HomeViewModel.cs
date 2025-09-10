using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Home";
        public string IconName { get; } = "Home";

        [ObservableProperty]
        private bool _isViewEnabled = true;
    }
}
