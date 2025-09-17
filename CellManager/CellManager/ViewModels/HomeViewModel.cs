using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     Represents the landing page which is always available and displays welcome messaging.
    /// </summary>
    public partial class HomeViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Home";
        public string IconName { get; } = "Home";

        [ObservableProperty]
        private bool _isViewEnabled = true;
    }
}
