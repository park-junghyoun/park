using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CellManager.Models;

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
        private TimeSpan _elapsedTime;

        [ObservableProperty]
        private TimeSpan _remainingTime;

        public ObservableCollection<string> RunLogs { get; } = new();

        public RelayCommand StartCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand StopCommand { get; }

        public RunViewModel()
        {
            StartCommand = new RelayCommand(() => { });
            PauseCommand = new RelayCommand(() => { });
            StopCommand = new RelayCommand(() => { });
        }
    }
}
