using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Messages;

namespace CellManager.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Home";
        public string IconName { get; } = "Home";

        [ObservableProperty]
        private bool _isViewEnabled = true;

        [RelayCommand]
        private void NavigateToCellLibrary() =>
            WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(typeof(CellLibraryViewModel)));

        [RelayCommand]
        private void NavigateToTestSetup() =>
            WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(typeof(TestSetupViewModel)));

        [RelayCommand]
        private void NavigateToSchedule() =>
            WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(typeof(ScheduleViewModel)));

        [RelayCommand]
        private void NavigateToRun() =>
            WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(typeof(RunViewModel)));
    }
}
