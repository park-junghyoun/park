using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     Supplies state for the help tab, which simply displays documentation links.
    /// </summary>
    public partial class HelpViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Help";
        public string IconName { get; } = "HelpCircle";

        [ObservableProperty]
        private bool _isViewEnabled = true;
    }
}