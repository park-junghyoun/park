using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    public partial class HelpViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Help";
        public string IconName { get; } = "HelpCircle";

        [ObservableProperty]
        private bool _isViewEnabled = true;
    }
}