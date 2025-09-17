using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     Placeholder VM representing a future display-focused feature area.
    /// </summary>
    public partial class DisplayViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Display";
        public string IconName { get; } = "Monitor";

        [ObservableProperty]
        private bool _isViewEnabled = false;
    }
}