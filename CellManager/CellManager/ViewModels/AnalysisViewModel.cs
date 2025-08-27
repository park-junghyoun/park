using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    public partial class AnalysisViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Analysis";
        public string IconName { get; } = "ChartAreaspline";

        [ObservableProperty]
        private bool _isViewEnabled = false;
    }
}