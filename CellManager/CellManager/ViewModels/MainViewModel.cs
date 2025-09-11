using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using CellManager.Services;
using CellManager.Messages;
using CellManager.Models;

namespace CellManager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ICellRepository _cellRepository;
        private readonly TestSetupViewModel _testSetupVm;
        private readonly ScheduleViewModel _scheduleVm;
        private readonly RunViewModel _runVm;
        private readonly IServerStatusService _serverStatusService;

        public string HeaderText { get; } = "Main";
        public string IconName { get; } = "ViewDashboard";

        [ObservableProperty] private bool _isViewEnabled = true;

        [ObservableProperty] private ObservableCollection<Cell> _availableCells = new();
        [ObservableProperty] private Cell _selectedCell;

        [ObservableProperty] private string _boardStatus = "Disconnected";
        [ObservableProperty] private string _serverStatus = "Disconnected";
        [ObservableProperty] private string _boardVersion = string.Empty;
        [ObservableProperty] private string _currentSchedule = string.Empty;
        [ObservableProperty] private string _currentProfile = string.Empty;

        [ObservableProperty] private ObservableCollection<ObservableObject> _navigationItems = new();
        [ObservableProperty] private ObservableObject _currentViewModel;

        public int CellLibraryCount => AvailableCells.Count;
        public int ScheduleCount => _scheduleVm.Schedules.Count;

        public MainViewModel(
            ICellRepository cellRepository,
            HomeViewModel homeVm,
            CellLibraryViewModel cellLibraryVm,
            TestSetupViewModel testSetupVm,
            ScheduleViewModel scheduleVm,
            RunViewModel runVm,
            AnalysisViewModel analysisVm,
            DataExportViewModel dataExportVm,
            SettingsViewModel settingsVm,
            HelpViewModel helpVm,
            IServerStatusService serverStatusService
        )
        {
            Debug.WriteLine("MainViewModel DI ctor");
            _cellRepository = cellRepository;
            _testSetupVm = testSetupVm;
            _scheduleVm = scheduleVm;
            _runVm = runVm;
            _serverStatusService = serverStatusService;
            BoardVersion = settingsVm.FirmwareVersion;

            NavigationItems.Add(homeVm);
            NavigationItems.Add(cellLibraryVm);
            NavigationItems.Add(testSetupVm);
            NavigationItems.Add(scheduleVm);
            NavigationItems.Add(runVm);
            NavigationItems.Add(analysisVm);
            NavigationItems.Add(dataExportVm);
            NavigationItems.Add(settingsVm);
            NavigationItems.Add(helpVm);
            CurrentViewModel = NavigationItems.FirstOrDefault();

            LoadCells();
            UpdateFeatureTabs();
            UpdateServerStatus();

            AvailableCells.CollectionChanged += (_, __) => OnPropertyChanged(nameof(CellLibraryCount));
            _scheduleVm.Schedules.CollectionChanged += (_, __) => OnPropertyChanged(nameof(ScheduleCount));

            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                SelectedCell = m.SelectedCell;
            });

            WeakReferenceMessenger.Default.Register<CellAddedMessage>(this, (r, m) =>
            {
                AvailableCells.Add(m.AddedCell);
                SelectedCell = m.AddedCell;
            });

            WeakReferenceMessenger.Default.Register<CellDeletedMessage>(this, (r, m) =>
            {
                var target = AvailableCells.FirstOrDefault(c => c.Id == m.DeletedCell.Id);
                if (target != null) AvailableCells.Remove(target);
                if (SelectedCell?.Id == m.DeletedCell.Id) SelectedCell = null;
            });

            WeakReferenceMessenger.Default.Register<BoardVersionChangedMessage>(this, (r, m) =>
            {
                BoardVersion = m.Version;
            });

            WeakReferenceMessenger.Default.Register<ScheduleChangedMessage>(this, (r, m) =>
            {
                CurrentSchedule = m.Schedule?.DisplayNameAndId ?? string.Empty;
            });

            WeakReferenceMessenger.Default.Register<ProfileChangedMessage>(this, (r, m) =>
            {
                CurrentProfile = m.Profile;
            });
        }

        private void LoadCells()
        {
            AvailableCells.Clear();
            foreach (var c in _cellRepository.LoadCells())
                AvailableCells.Add(c);

            if (SelectedCell != null)
                WeakReferenceMessenger.Default.Send(new CellSelectedMessage(SelectedCell));
        }

        private void UpdateFeatureTabs()
        {
            var enabled = SelectedCell != null;
            _testSetupVm.IsViewEnabled = enabled;
            _scheduleVm.IsViewEnabled = enabled;
            _runVm.IsViewEnabled = enabled;
        }

        private async void UpdateServerStatus()
        {
            ServerStatus = await _serverStatusService.IsServerAvailableAsync() ? "Connected" : "Disconnected";
        }

        partial void OnSelectedCellChanged(Cell value)
        {
            UpdateFeatureTabs();
            OnPropertyChanged(nameof(ScheduleCount));
        }
    }
}