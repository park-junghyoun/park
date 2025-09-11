using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Models;
using CellManager.Messages;

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

        public RunViewModel()
        {
            StartCommand = new RelayCommand(StartSchedule);
            PauseCommand = new RelayCommand(() => { });
            StopCommand = new RelayCommand(() => { });
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
            }
        }

        partial void OnCurrentProfileChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(new ProfileChangedMessage(value));
        }
    }
}
