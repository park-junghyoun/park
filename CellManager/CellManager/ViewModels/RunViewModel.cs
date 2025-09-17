using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Models;
using CellManager.Messages;
using CellManager.Models.TestProfile;
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
        public ObservableCollection<StepTemplate> TimelineSteps { get; } = new();

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
        private StepTemplate? _currentTimelineStep;

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
        private readonly IChargeProfileRepository? _chargeRepo;
        private readonly IDischargeProfileRepository? _dischargeRepo;
        private readonly IRestProfileRepository? _restRepo;
        private readonly IOcvProfileRepository? _ocvRepo;
        private readonly IEcmPulseProfileRepository? _ecmRepo;

        private readonly Dictionary<int, StepTemplate> _profileTemplates = new();

        public RunViewModel()
            : this(null, null, null, null, null, null)
        {
            BuildDesignTimeData();
        }

        public RunViewModel(IScheduleRepository? scheduleRepo)
            : this(scheduleRepo, null, null, null, null, null)
        {
        }

        public RunViewModel(
            IScheduleRepository? scheduleRepo,
            IChargeProfileRepository? chargeRepo,
            IDischargeProfileRepository? dischargeRepo,
            IRestProfileRepository? restRepo,
            IOcvProfileRepository? ocvRepo,
            IEcmPulseProfileRepository? ecmRepo)
        {
            _scheduleRepo = scheduleRepo;
            _chargeRepo = chargeRepo;
            _dischargeRepo = dischargeRepo;
            _restRepo = restRepo;
            _ocvRepo = ocvRepo;
            _ecmRepo = ecmRepo;

            StartCommand = new RelayCommand(StartSchedule);
            PauseCommand = new RelayCommand(() => { });
            StopCommand = new RelayCommand(StopSchedule);

            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                OnCellSelected(m.SelectedCell);
            });
        }

        private void LoadSchedules(Cell? cell)
        {
            AvailableSchedules.Clear();
            if (_scheduleRepo == null || cell == null)
            {
                SelectedSchedule = null;
                return;
            }

            foreach (var sched in _scheduleRepo.Load(cell.Id))
                AvailableSchedules.Add(sched);

            SelectedSchedule = AvailableSchedules.FirstOrDefault();
        }

        private void OnCellSelected(Cell? cell)
        {
            LoadStepTemplates(cell);
            LoadSchedules(cell);
        }

        private void LoadStepTemplates(Cell? cell)
        {
            _profileTemplates.Clear();

            if (cell == null)
                return;

            if (_chargeRepo != null)
            {
                foreach (var profile in _chargeRepo.Load(cell.Id))
                {
                    _profileTemplates[profile.Id] = CreateProfileTemplate(
                        profile.Id,
                        profile.Name,
                        "BatteryPlus",
                        profile.PreviewText,
                        ScheduleTimeCalculator.EstimateDuration(cell, TestProfileType.Charge, profile) ?? TimeSpan.Zero);
                }
            }

            if (_dischargeRepo != null)
            {
                foreach (var profile in _dischargeRepo.Load(cell.Id))
                {
                    _profileTemplates[profile.Id] = CreateProfileTemplate(
                        profile.Id,
                        profile.Name,
                        "BatteryMinus",
                        profile.PreviewText,
                        ScheduleTimeCalculator.EstimateDuration(cell, TestProfileType.Discharge, profile) ?? TimeSpan.Zero);
                }
            }

            if (_restRepo != null)
            {
                foreach (var profile in _restRepo.Load(cell.Id))
                {
                    _profileTemplates[profile.Id] = CreateProfileTemplate(
                        profile.Id,
                        profile.Name,
                        "BatteryEco",
                        profile.PreviewText,
                        ScheduleTimeCalculator.EstimateDuration(cell, TestProfileType.Rest, profile) ?? TimeSpan.Zero);
                }
            }

            if (_ocvRepo != null)
            {
                foreach (var profile in _ocvRepo.Load(cell.Id))
                {
                    _profileTemplates[profile.Id] = CreateProfileTemplate(
                        profile.Id,
                        profile.Name,
                        "StairsDown",
                        profile.PreviewText,
                        ScheduleTimeCalculator.EstimateDuration(cell, TestProfileType.OCV, profile) ?? TimeSpan.Zero);
                }
            }

            if (_ecmRepo != null)
            {
                foreach (var profile in _ecmRepo.Load(cell.Id))
                {
                    _profileTemplates[profile.Id] = CreateProfileTemplate(
                        profile.Id,
                        profile.Name,
                        "Pulse",
                        profile.PreviewText,
                        ScheduleTimeCalculator.EstimateDuration(cell, TestProfileType.ECM, profile) ?? TimeSpan.Zero);
                }
            }
        }

        private void UpdateTimelineSteps(Schedule? schedule)
        {
            TimelineSteps.Clear();

            if (schedule == null)
                return;

            var steps = new List<StepTemplate>();

            foreach (var id in schedule.TestProfileIds)
                steps.Add(CloneTemplateForTimeline(id));

            if (schedule.LoopStartIndex > 0)
            {
                var insertIndex = Math.Clamp(schedule.LoopStartIndex - 1, 0, steps.Count);
                steps.Insert(insertIndex, CreateLoopStep(true));
            }

            if (schedule.LoopEndIndex > 0)
            {
                var insertIndex = Math.Clamp(schedule.LoopEndIndex - 1, 0, steps.Count);
                steps.Insert(insertIndex, CreateLoopStep(false));
            }

            for (var i = 0; i < steps.Count; i++)
                steps[i].StepNumber = i + 1;

            foreach (var step in steps)
                TimelineSteps.Add(step);
        }

        private StepTemplate CloneTemplateForTimeline(int id)
        {
            if (_profileTemplates.TryGetValue(id, out var template))
                return CloneStep(template);

            return new StepTemplate
            {
                Id = id,
                Name = $"Profile {id}",
                IconKind = "HelpCircleOutline",
                Parameters = "Profile details unavailable",
                Duration = TimeSpan.Zero,
                Kind = StepKind.Profile
            };
        }

        private static StepTemplate CloneStep(StepTemplate template)
        {
            return new StepTemplate
            {
                Id = template.Id,
                Name = template.Name,
                IconKind = template.IconKind,
                Parameters = template.Parameters,
                Duration = template.Duration,
                Kind = template.Kind
            };
        }

        private static StepTemplate CreateLoopStep(bool isStart)
        {
            return new StepTemplate
            {
                Id = 0,
                Name = isStart ? "Loop Start" : "Loop End",
                IconKind = "Repeat",
                Parameters = string.Empty,
                Duration = TimeSpan.Zero,
                Kind = isStart ? StepKind.LoopStart : StepKind.LoopEnd
            };
        }

        private static StepTemplate CreateProfileTemplate(int id, string? name, string iconKind, string? parameters, TimeSpan duration)
        {
            return new StepTemplate
            {
                Id = id,
                Name = string.IsNullOrWhiteSpace(name) ? $"Profile {id}" : name,
                IconKind = iconKind,
                Parameters = parameters ?? string.Empty,
                Duration = duration,
                Kind = StepKind.Profile
            };
        }

        private void BuildDesignTimeData()
        {
            _profileTemplates.Clear();

            var samples = new[]
            {
                CreateProfileTemplate(1, "Charge", "BatteryPlus", "0.5A → 4.2V | 01:00:00", TimeSpan.FromHours(1)),
                CreateProfileTemplate(2, "Discharge", "BatteryMinus", "0.5A → 3.0V | 00:30:00", TimeSpan.FromMinutes(30)),
                CreateProfileTemplate(3, "Rest", "BatteryEco", "00:10:00", TimeSpan.FromMinutes(10))
            };

            foreach (var sample in samples)
                _profileTemplates[sample.Id] = sample;

            AvailableSchedules.Clear();
            var schedule = new Schedule
            {
                Id = 1,
                Name = "Sample Schedule",
                TestProfileIds = new List<int> { 1, 2, 3 }
            };
            AvailableSchedules.Add(schedule);
            SelectedSchedule = schedule;
            RemainingTime = TimeSpan.FromTicks(TimelineSteps.Sum(s => s.Duration.Ticks));
        }

        private void StartSchedule()
        {
            if (SelectedSchedule == null)
                return;

            WeakReferenceMessenger.Default.Send(new ScheduleChangedMessage(SelectedSchedule));

            CurrentTimelineStep = TimelineSteps.FirstOrDefault(s => s.Kind == StepKind.Profile)
                ?? TimelineSteps.FirstOrDefault();
            Progress = 0;
            ElapsedTime = TimeSpan.Zero;
            RemainingTime = TimeSpan.FromTicks(TimelineSteps.Sum(s => s.Duration.Ticks));

            WeakReferenceMessenger.Default.Send(new TestStatusChangedMessage("Testing"));
        }

        private void StopSchedule()
        {
            CurrentTimelineStep = null;
            Progress = 0;
            ElapsedTime = TimeSpan.Zero;
            RemainingTime = TimeSpan.Zero;
            WeakReferenceMessenger.Default.Send(new TestStatusChangedMessage(string.Empty));
        }

        partial void OnSelectedScheduleChanged(Schedule? value)
        {
            UpdateTimelineSteps(value);
            RemainingTime = TimeSpan.FromTicks(TimelineSteps.Sum(s => s.Duration.Ticks));
            ElapsedTime = TimeSpan.Zero;
            CurrentTimelineStep = null;
        }

        partial void OnCurrentTimelineStepChanged(StepTemplate? value)
        {
            if (value != null)
            {
                CurrentProfile = $"{value.Name} (ID: {value.Id})";
                CurrentStep = value.StepNumber;
            }
            else
            {
                CurrentProfile = string.Empty;
                CurrentStep = 0;
            }
        }

        partial void OnCurrentProfileChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(new ProfileChangedMessage(value));
        }
    }
}
