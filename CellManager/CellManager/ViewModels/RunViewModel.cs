using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Messages;
using CellManager.Models;
using CellManager.Services;

namespace CellManager.ViewModels
{
    public partial class RunViewModel : ObservableObject
    {
        private readonly IScheduleRepository? _scheduleRepo;
        private readonly Random _random = new();
        private Timer? _timer;
        private bool _isPaused;
        private DateTime _startTime;

        public string HeaderText { get; } = "Run";
        public string IconName { get; } = "Play";

        [ObservableProperty]
        private bool _isViewEnabled = true;

        [ObservableProperty]
        private bool _isRunning;

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

        public RunViewModel() : this(null) { }

        public RunViewModel(IScheduleRepository? scheduleRepo)
        {
            _scheduleRepo = scheduleRepo;

            StartCommand = new RelayCommand(Start, () => !IsRunning && SelectedSchedule != null);
            PauseCommand = new RelayCommand(TogglePause, () => IsRunning);
            StopCommand = new RelayCommand(Stop, () => IsRunning);

            if (_scheduleRepo == null)
            {
                // Provide a demo schedule for design-time or tests
                AvailableSchedules.Add(new Schedule { Id = 1, Name = "Demo", EstimatedDuration = TimeSpan.FromSeconds(5) });
                SelectedSchedule = AvailableSchedules.FirstOrDefault();
            }
            else
            {
                WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
                {
                    LoadSchedules(m.SelectedCell.Id);
                });
            }
        }

        private void LoadSchedules(int cellId)
        {
            AvailableSchedules.Clear();
            if (_scheduleRepo == null) return;
            foreach (var sched in _scheduleRepo.Load(cellId))
                AvailableSchedules.Add(sched);
            SelectedSchedule = AvailableSchedules.FirstOrDefault();
        }

        private void Start()
        {
            if (SelectedSchedule == null) return;
            RunLogs.Add($"Started {SelectedSchedule.Name}");
            _startTime = DateTime.Now;
            _timer = new Timer(200);
            _timer.Elapsed += OnTimer;
            _timer.Start();
            IsRunning = true;
            UpdateCommandStates();
        }

        private void OnTimer(object? sender, ElapsedEventArgs e)
        {
            if (_isPaused || SelectedSchedule == null) return;

            var elapsed = DateTime.Now - _startTime;
            ElapsedTime = elapsed;
            var total = SelectedSchedule.EstimatedDuration.TotalSeconds;
            if (total > 0)
            {
                Progress = Math.Min(100, elapsed.TotalSeconds / total * 100);
                RemainingTime = TimeSpan.FromSeconds(Math.Max(0, total - elapsed.TotalSeconds));
            }

            // Update mock board metrics
            BoardVoltage = 3.0 + _random.NextDouble();
            BoardCurrent = 0.5 + _random.NextDouble();
            BoardTemperature = 25 + _random.NextDouble() * 5;
            IsBoardConnected = true;

            if (Progress >= 100)
            {
                RunLogs.Add($"Completed {SelectedSchedule.Name}");
                Stop();
            }
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            RunLogs.Add(_isPaused ? "Paused" : "Resumed");
        }

        private void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            IsRunning = false;
            _isPaused = false;
            Progress = 0;
            CurrentStep = 0;
            ElapsedTime = TimeSpan.Zero;
            RemainingTime = TimeSpan.Zero;
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            StartCommand.NotifyCanExecuteChanged();
            PauseCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedScheduleChanged(Schedule? value) => StartCommand.NotifyCanExecuteChanged();

        partial void OnIsRunningChanged(bool value) => UpdateCommandStates();
    }
}

