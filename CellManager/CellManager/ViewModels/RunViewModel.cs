using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Models;
using CellManager.Messages;
using CellManager.Services;

namespace CellManager.ViewModels
{
    public partial class RunViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Run";
        public string IconName { get; } = "Play";

        [ObservableProperty]
        private bool _isViewEnabled = true;

        public ObservableCollection<Schedule> AvailableSchedules { get; } = new();

        [ObservableProperty]
        private Schedule? _selectedSchedule;

        [ObservableProperty]
        private double _boardVoltage;

        [ObservableProperty]
        private double _boardCurrent;

        [ObservableProperty]
        private double _boardTemperature;

        [ObservableProperty]
        private bool _isBoardConnected;

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private int _currentStep;

        [ObservableProperty]
        private string _currentProfile = string.Empty;

        [ObservableProperty]
        private TimeSpan _elapsedTime;

        [ObservableProperty]
        private TimeSpan _remainingTime;

        public ObservableCollection<string> RunLogs { get; } = new();

        public RelayCommand StartCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand StopCommand { get; }

        private readonly IScheduleRepository? _scheduleRepo;

        public RunViewModel(IScheduleRepository? scheduleRepo)
        {
            _scheduleRepo = scheduleRepo;

            StartCommand = new RelayCommand(StartSchedule);
            PauseCommand = new RelayCommand(() => { });
            StopCommand = new RelayCommand(StopSchedule);

            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                LoadSchedules(m.SelectedCell);
            });
        }

        private void LoadSchedules(Cell? cell)
        {
            AvailableSchedules.Clear();
            if (_scheduleRepo == null || cell == null) return;
            foreach (var sched in _scheduleRepo.Load(cell.Id))
                AvailableSchedules.Add(sched);
            SelectedSchedule = AvailableSchedules.FirstOrDefault();
        }

        private void StartSchedule()
        {
            if (SelectedSchedule != null)
            {
                WeakReferenceMessenger.Default.Send(new ScheduleChangedMessage(SelectedSchedule));

                if (SelectedSchedule.TestProfileIds.Count > 0)
                {
                    CurrentProfile = $"Profile ID: {SelectedSchedule.TestProfileIds[0]}";
                }

                WeakReferenceMessenger.Default.Send(new TestStatusChangedMessage("Testing"));
            }
        }

        private void StopSchedule()
        {
            WeakReferenceMessenger.Default.Send(new TestStatusChangedMessage(string.Empty));
        }

        partial void OnCurrentProfileChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(new ProfileChangedMessage(value));
        }
    }
}
