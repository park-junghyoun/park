using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Messages;
using CellManager.Models;
using CellManager.Models.TestProfile;
using CellManager.Services;

namespace CellManager.ViewModels
{
    /// <summary>Identifies the type of step represented in the schedule sequence.</summary>
    public enum StepKind
    {
        Profile,
        LoopStart,
        LoopEnd
    }

    /// <summary>
    ///     Represents a selectable step from the library, including metadata used during scheduling.
    /// </summary>
    public class StepTemplate : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IconKind { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public StepKind Kind { get; set; } = StepKind.Profile;
        private int _stepNumber;
        public int StepNumber
        {
            get => _stepNumber;
            set => SetProperty(ref _stepNumber, value);
        }
    }

    /// <summary>Grouping of step templates displayed within the library panel.</summary>
    public class StepGroup : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string IconKind { get; set; } = string.Empty;
        public ObservableCollection<StepTemplate> Steps { get; } = new();
    }

    /// <summary>
    ///     Allows operators to compose ordered sequences of profile steps into executable schedules.
    /// </summary>
    public partial class ScheduleViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Schedule";
        public string IconName { get; } = "Calendar";

        private readonly IChargeProfileRepository? _chargeRepo;
        private readonly IDischargeProfileRepository? _dischargeRepo;
        private readonly IRestProfileRepository? _restRepo;
        private readonly IOcvProfileRepository? _ocvRepo;
        private readonly IEcmPulseProfileRepository? _ecmRepo;
        private readonly IScheduleRepository? _scheduleRepo;

        [ObservableProperty]
        private bool _isViewEnabled = true;

        public ObservableCollection<StepGroup> StepLibrary { get; } = new();
        public ObservableCollection<StepTemplate> Sequence { get; } = new();
        public ObservableCollection<Schedule> Schedules { get; } = new();

        [ObservableProperty] private Schedule? _selectedSchedule;

        [ObservableProperty] private string _scheduleName = "New Schedule";
        [ObservableProperty] private int _repeatCount = 1;
        [ObservableProperty] private int _loopStartIndex;
        [ObservableProperty] private int _loopEndIndex;
        [ObservableProperty] private TimeSpan _totalDuration;
        [ObservableProperty] private Cell? _selectedCell;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ScheduleStartDateTime))]
        [NotifyPropertyChangedFor(nameof(ScheduleEndDateTime))]
        private DateTime _scheduleStartDate = DateTime.Now.Date;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ScheduleStartDateTime))]
        [NotifyPropertyChangedFor(nameof(ScheduleEndDateTime))]
        private TimeSpan _scheduleStartTime = DateTime.Now.TimeOfDay;

        private string _scheduleSummaryText = string.Empty;
        public string ScheduleSummaryText
        {
            get => _scheduleSummaryText;
            private set => SetProperty(ref _scheduleSummaryText, value);
        }

        private string _loopSummaryText = string.Empty;
        public string LoopSummaryText
        {
            get => _loopSummaryText;
            private set => SetProperty(ref _loopSummaryText, value);
        }

        private bool _isLoopValid = true;
        public bool IsLoopValid
        {
            get => _isLoopValid;
            private set => SetProperty(ref _isLoopValid, value);
        }

        public string LoopRangeDisplay
        {
            get
            {
                if (LoopStartIndex <= 0 || LoopEndIndex <= 0)
                    return "Not set";
                var range = $"{LoopStartIndex} → {LoopEndIndex}";
                return IsLoopValid ? range : $"{range} (invalid)";
            }
        }

        public DateTime ScheduleStartDateTime => ScheduleStartDate.Date + ScheduleStartTime;

        public DateTime ScheduleEndDateTime => ScheduleStartDateTime + TotalDuration;

        public DateTime? ScheduleStartTimePickerValue
        {
            get => DateTime.Today.Add(ScheduleStartTime);
            set => ScheduleStartTime = (value ?? DateTime.Today).TimeOfDay;
        }

        public ObservableCollection<ScheduleCalendarDay> CalendarDays { get; } = new();

        public RelayCommand<StepTemplate> RemoveStepCommand { get; }
        public RelayCommand SaveScheduleCommand { get; }
        public RelayCommand AddScheduleCommand { get; }
        public RelayCommand<Schedule> DeleteScheduleCommand { get; }

        public ScheduleViewModel() : this(null, null, null, null, null, null) { }

        public ScheduleViewModel(
            IChargeProfileRepository? chargeRepo,
            IDischargeProfileRepository? dischargeRepo,
            IEcmPulseProfileRepository? ecmRepo,
            IOcvProfileRepository? ocvRepo,
            IRestProfileRepository? restRepo,
            IScheduleRepository? scheduleRepo)
        {
            _chargeRepo = chargeRepo;
            _dischargeRepo = dischargeRepo;
            _restRepo = restRepo;
            _ocvRepo = ocvRepo;
            _ecmRepo = ecmRepo;
            _scheduleRepo = scheduleRepo;

            Sequence.CollectionChanged += (_, __) =>
            {
                UpdateTotalDuration();
                UpdateStepNumbers();
                SaveScheduleCommand?.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanSaveSchedule));
            };

            RemoveStepCommand = new RelayCommand<StepTemplate>(s => Sequence.Remove(s));
            SaveScheduleCommand = new RelayCommand(SaveSchedule, () => CanSaveSchedule);
            AddScheduleCommand = new RelayCommand(AddSchedule);
            DeleteScheduleCommand = new RelayCommand<Schedule>(DeleteSchedule, s => s != null);

            if (_scheduleRepo == null)
            {
                BuildMockSchedules();
            }

            if (_chargeRepo == null)
            {
                BuildMockLibrary();
            }
            else
            {
                WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
                {
                    SelectedCell = m.SelectedCell;
                });
            }

            WeakReferenceMessenger.Default.Register<TestProfilesUpdatedMessage>(this, (r, m) =>
            {
                if (SelectedCell?.Id == m.Value)
                    LoadStepLibrary();
            });

            UpdateTotalDuration();
            ResetScheduleStartToNow();
        }

        /// <summary>Reloads the schedule and library data when the active cell changes.</summary>
        partial void OnSelectedCellChanged(Cell? value)
        {
            LoadStepLibrary();
            LoadSchedules();
        }

        /// <summary>
        ///     Refreshes schedules from the repository and optionally reselects the previously active schedule.
        /// </summary>
        private void LoadSchedules(int? preferredScheduleId = null)
        {
            var previousId = preferredScheduleId ?? SelectedSchedule?.Id;
            Schedules.Clear();
            SelectedSchedule = null;
            if (_scheduleRepo == null || SelectedCell == null) return;
            foreach (var sched in _scheduleRepo.Load(SelectedCell.Id))
                Schedules.Add(sched);
            if (previousId.HasValue)
                SelectedSchedule = Schedules.FirstOrDefault(s => s.Id == previousId.Value);
            if (SelectedSchedule == null)
                SelectedSchedule = Schedules.FirstOrDefault();
            UpdateScheduleDurations();
        }

        /// <summary>Creates a few in-memory schedules used when no repository is wired up.</summary>
        private void BuildMockSchedules()
        {
            Schedules.Add(new Schedule { Id = 1, CellId = 0, Ordering = 1, Name = "Schedule A", TestProfileIds = { 1, 2 } });
            Schedules.Add(new Schedule { Id = 2, CellId = 0, Ordering = 2, Name = "Schedule B", TestProfileIds = { 3 } });
        }

        /// <summary>Populates the step library with sample entries for design-time usage.</summary>
        private void BuildMockLibrary()
        {
            var id = 1;
            StepLibrary.Add(new StepGroup
            {
                Name = "Charge",
                IconKind = "BatteryPlus",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "Charge",
                        IconKind = "BatteryPlus",
                        Parameters = "0.5A → 4.2V | 01:00:00",
                        Duration = TimeSpan.FromHours(1)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "Discharge",
                IconKind = "BatteryMinus",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "Discharge",
                        IconKind = "BatteryMinus",
                        Parameters = "0.5A → 3.0V | 00:30:00",
                        Duration = TimeSpan.FromMinutes(30)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "Rest",
                IconKind = "BatteryEco",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "Rest",
                        IconKind = "BatteryEco",
                        Parameters = "00:10:00",
                        Duration = TimeSpan.FromMinutes(10)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "OCV",
                IconKind = "StairsDown",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "OCV",
                        IconKind = "StairsDown",
                        Parameters = "01:00:00",
                        Duration = TimeSpan.FromHours(1)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "ECM",
                IconKind = "Pulse",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "ECM",
                        IconKind = "Pulse",
                        Parameters = "0.2A 00:05:00",
                        Duration = TimeSpan.FromMinutes(5)
                    }
                }
            });

            AddLoopControls();
            UpdateScheduleDurations();
        }

        /// <summary>
        ///     Loads available profile steps for the selected cell and rebuilds the loop controls.
        /// </summary>
        private void LoadStepLibrary()
        {
            StepLibrary.Clear();
            if (SelectedCell == null)
            {
                AddLoopControls();
                return;
            }

            if (_chargeRepo != null)
            {
                var chargeGroup = new StepGroup { Name = "Charge", IconKind = "BatteryPlus" };
                foreach (var p in _chargeRepo.Load(SelectedCell.Id))
                {
                    chargeGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "BatteryPlus",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.Charge, p) ?? TimeSpan.Zero
                    });
                }
                if (chargeGroup.Steps.Any()) StepLibrary.Add(chargeGroup);
            }

            if (_dischargeRepo != null)
            {
                var disGroup = new StepGroup { Name = "Discharge", IconKind = "BatteryMinus" };
                foreach (var p in _dischargeRepo.Load(SelectedCell.Id))
                {
                    disGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "BatteryMinus",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.Discharge, p) ?? TimeSpan.Zero
                    });
                }
                if (disGroup.Steps.Any()) StepLibrary.Add(disGroup);
            }

            if (_restRepo != null)
            {
                var restGroup = new StepGroup { Name = "Rest", IconKind = "BatteryEco" };
                foreach (var p in _restRepo.Load(SelectedCell.Id))
                {
                    restGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "BatteryEco",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.Rest, p) ?? TimeSpan.Zero
                    });
                }
                if (restGroup.Steps.Any()) StepLibrary.Add(restGroup);
            }

            if (_ocvRepo != null)
            {
                var ocvGroup = new StepGroup { Name = "OCV", IconKind = "StairsDown" };
                foreach (var p in _ocvRepo.Load(SelectedCell.Id))
                {
                    ocvGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "StairsDown",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.OCV, p) ?? TimeSpan.Zero
                    });
                }
                if (ocvGroup.Steps.Any()) StepLibrary.Add(ocvGroup);
            }

            if (_ecmRepo != null)
            {
                var ecmGroup = new StepGroup { Name = "ECM", IconKind = "Pulse" };
                foreach (var p in _ecmRepo.Load(SelectedCell.Id))
                {
                    ecmGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "Pulse",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.ECM, p) ?? TimeSpan.Zero
                    });
                }
                if (ecmGroup.Steps.Any()) StepLibrary.Add(ecmGroup);
            }

            AddLoopControls();

            if (SelectedSchedule != null)
                OnSelectedScheduleChanged(SelectedSchedule);
            UpdateScheduleDurations();
        }

        /// <summary>Adds virtual steps that represent the start and end of a loop block.</summary>
        private void AddLoopControls()
        {
            var loopGroup = new StepGroup { Name = "Loop", IconKind = "Repeat" };
            loopGroup.Steps.Add(new StepTemplate { Id = 0, Name = "Loop Start", IconKind = "Repeat", Kind = StepKind.LoopStart });
            loopGroup.Steps.Add(new StepTemplate { Id = 0, Name = "Loop End", IconKind = "Repeat", Kind = StepKind.LoopEnd });
            StepLibrary.Add(loopGroup);
        }

        /// <summary>Inserts a copy of the provided template into the sequence at the given index.</summary>
        public void InsertStep(StepTemplate template, int index)
        {
            if (template.Kind == StepKind.LoopStart && Sequence.Any(s => s.Kind == StepKind.LoopStart))
                return;
            if (template.Kind == StepKind.LoopEnd && Sequence.Any(s => s.Kind == StepKind.LoopEnd))
                return;
            var clone = new StepTemplate
            {
                Id = template.Id,
                Name = template.Name,
                IconKind = template.IconKind,
                Parameters = template.Parameters,
                Duration = template.Duration,
                Kind = template.Kind
            };
            if (index < 0 || index > Sequence.Count)
                Sequence.Add(clone);
            else
                Sequence.Insert(index, clone);
        }

        /// <summary>Moves an existing step to a new index, adjusting for the removal offset.</summary>
        public void MoveStep(StepTemplate step, int index)
        {
            var oldIndex = Sequence.IndexOf(step);
            if (oldIndex < 0) return;
            if (oldIndex < index) index--;
            if (index < 0) index = 0;
            if (index >= Sequence.Count) index = Sequence.Count - 1;
            Sequence.Move(oldIndex, index);
        }

        /// <summary>Aggregates the duration of all steps to update the total runtime.</summary>
        private void UpdateTotalDuration()
        {
            var totalTicks = Sequence.Sum(s => s.Duration.Ticks);

            var repeatCount = Math.Max(1, RepeatCount);
            if (repeatCount > 1)
            {
                var loopTicks = CalculateLoopSegmentTicksFromSequence();
                if (loopTicks > 0)
                    totalTicks += loopTicks * (repeatCount - 1);
            }

            TotalDuration = new TimeSpan(totalTicks);
            if (SelectedSchedule != null)
                SelectedSchedule.EstimatedDuration = TotalDuration;
            UpdateLoopIndices();
            UpdateScheduleSummaryText();
            RefreshLoopSummary();
            RefreshCalendarSchedule();
            OnPropertyChanged(nameof(CanSaveSchedule));
        }

        private void ResetScheduleStartToNow()
        {
            var now = DateTime.Now;
            ScheduleStartDate = now.Date;
            ScheduleStartTime = now.TimeOfDay;
        }

        /// <summary>Determines the total duration encompassed by the current loop markers.</summary>
        private long CalculateLoopSegmentTicksFromSequence()
        {
            var loopStart = Sequence.IndexOf(Sequence.FirstOrDefault(s => s.Kind == StepKind.LoopStart));
            var loopEnd = Sequence.IndexOf(Sequence.FirstOrDefault(s => s.Kind == StepKind.LoopEnd));

            if (loopStart < 0 || loopEnd < 0 || loopEnd <= loopStart)
                return 0;

            return Sequence
                .Skip(loopStart + 1)
                .Take(loopEnd - loopStart - 1)
                .Sum(s => s.Duration.Ticks);
        }

        /// <summary>Renumbers the steps to maintain human-readable ordering.</summary>
        private void UpdateStepNumbers()
        {
            for (int i = 0; i < Sequence.Count; i++)
                Sequence[i].StepNumber = i + 1;
        }

        /// <summary>Calculates the estimated duration for each saved schedule.</summary>
        private void UpdateScheduleDurations()
        {
            foreach (var sched in Schedules)
            {
                var profileDurations = sched.TestProfileIds
                    .Select(id => StepLibrary.SelectMany(g => g.Steps)
                        .FirstOrDefault(s => s.Id == id && s.Kind == StepKind.Profile)?.Duration.Ticks ?? 0)
                    .ToList();

                var ticks = profileDurations.Sum();

                var repeatCount = Math.Max(1, sched.RepeatCount);
                if (repeatCount > 1 && sched.LoopStartIndex > 0 && sched.LoopEndIndex > 0)
                {
                    var loopTicks = CalculateLoopSegmentTicks(profileDurations, sched.LoopStartIndex, sched.LoopEndIndex);
                    if (loopTicks > 0)
                        ticks += loopTicks * (repeatCount - 1);
                }

                sched.EstimatedDuration = new TimeSpan(ticks);
            }
        }

        /// <summary>Calculates the ticks contained between the loop markers for the supplied durations.</summary>
        private static long CalculateLoopSegmentTicks(IList<long> profileDurations, int loopStartIndex, int loopEndIndex)
        {
            if (profileDurations.Count == 0 || loopStartIndex <= 0 || loopEndIndex <= loopStartIndex)
                return 0;

            var sequence = profileDurations.Select(d => (long?)d).ToList();

            var startInsertIndex = Math.Min(loopStartIndex - 1, sequence.Count);
            sequence.Insert(startInsertIndex, null);

            var endInsertIndex = Math.Min(loopEndIndex - 1, sequence.Count);
            sequence.Insert(endInsertIndex, null);

            var startIndex = sequence.IndexOf(null);
            if (startIndex < 0)
                return 0;

            var endIndex = sequence.FindIndex(startIndex + 1, value => value == null);
            if (endIndex < 0)
                endIndex = sequence.LastIndexOf(null);

            if (endIndex <= startIndex)
                return 0;

            long loopTicks = 0;
            for (var i = startIndex + 1; i < endIndex; i++)
            {
                if (sequence[i].HasValue)
                    loopTicks += sequence[i]!.Value;
            }

            return loopTicks;
        }

        /// <summary>Updates cached indices for loop start and end markers in the sequence.</summary>
        private void UpdateLoopIndices()
        {
            LoopStartIndex = Sequence.IndexOf(Sequence.FirstOrDefault(s => s.Kind == StepKind.LoopStart)) + 1;
            LoopEndIndex = Sequence.IndexOf(Sequence.FirstOrDefault(s => s.Kind == StepKind.LoopEnd)) + 1;
        }

        /// <summary>Determines whether the current schedule configuration can be persisted.</summary>
        public bool CanSaveSchedule => SelectedSchedule != null && TryGetNormalizedLoopBounds(out _, out _);

        /// <summary>Loads the steps of the selected schedule into the editable sequence.</summary>
        partial void OnSelectedScheduleChanged(Schedule? value)
        {
            Sequence.Clear();
            if (value != null)
            {
                ScheduleName = value.Name;
                RepeatCount = value.RepeatCount;
                foreach (var id in value.TestProfileIds)
                {
                    var template = StepLibrary.SelectMany(g => g.Steps).FirstOrDefault(s => s.Id == id && s.Kind == StepKind.Profile);
                    if (template != null)
                        InsertStep(template, -1);
                }
                var startTemplate = StepLibrary.SelectMany(g => g.Steps).FirstOrDefault(s => s.Kind == StepKind.LoopStart);
                var endTemplate = StepLibrary.SelectMany(g => g.Steps).FirstOrDefault(s => s.Kind == StepKind.LoopEnd);
                if (value.LoopStartIndex > 0 && startTemplate != null)
                    InsertStep(startTemplate, Math.Min(value.LoopStartIndex - 1, Sequence.Count));
                if (value.LoopEndIndex > 0 && endTemplate != null)
                    InsertStep(endTemplate, Math.Min(value.LoopEndIndex - 1, Sequence.Count));
                UpdateLoopIndices();
            }
            else
            {
                RepeatCount = 1;
                LoopStartIndex = 0;
                LoopEndIndex = 0;
            }
            DeleteScheduleCommand.NotifyCanExecuteChanged();
            SaveScheduleCommand?.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSaveSchedule));
            ResetScheduleStartToNow();
        }

        /// <summary>Creates a new blank schedule and selects it for editing.</summary>
        private void AddSchedule()
        {
            var newOrdering = Schedules.Any() ? Schedules.Max(s => s.Ordering) + 1 : 1;
            var sched = new Schedule { Ordering = newOrdering, Name = $"Schedule {newOrdering}", CellId = SelectedCell?.Id ?? 0 };
            Schedules.Add(sched);
            SelectedSchedule = sched;
            Sequence.Clear();
            ScheduleName = sched.Name;
            RepeatCount = 1;
            LoopStartIndex = 0;
            LoopEndIndex = 0;
        }

        /// <summary>Persists the current schedule to the repository and notifies listeners.</summary>
        private void SaveSchedule()
        {
            if (SelectedSchedule == null) return;
            if (!TryGetNormalizedLoopBounds(out var normalizedStart, out var normalizedEnd))
                return;
            SelectedSchedule.Name = ScheduleName;
            SelectedSchedule.TestProfileIds = Sequence.Where(s => s.Kind == StepKind.Profile).Select(s => s.Id).ToList();
            RepeatCount = Math.Max(1, RepeatCount);
            SelectedSchedule.RepeatCount = RepeatCount;
            LoopStartIndex = normalizedStart;
            LoopEndIndex = normalizedEnd;
            SelectedSchedule.LoopStartIndex = LoopStartIndex;
            SelectedSchedule.LoopEndIndex = LoopEndIndex;
            if (SelectedCell != null && _scheduleRepo != null)
            {
                SelectedSchedule.CellId = SelectedCell.Id;
                var savedId = _scheduleRepo.Save(SelectedCell.Id, SelectedSchedule);
                if (savedId == 0)
                    return;
                SelectedSchedule.Id = savedId;
                LoadSchedules(savedId);
                if (SelectedCell.Id > 0)
                    WeakReferenceMessenger.Default.Send(new SchedulesUpdatedMessage(SelectedCell.Id));
            }
        }

        /// <summary>Deletes the provided schedule and updates the remaining selection.</summary>
        private void DeleteSchedule(Schedule? schedule)
        {
            var target = schedule ?? SelectedSchedule;
            if (target == null) return;
            if (SelectedCell != null)
                _scheduleRepo?.Delete(SelectedCell.Id, target.Id);
            var index = Schedules.IndexOf(target);
            Schedules.Remove(target);
            SelectedSchedule = index < Schedules.Count ? Schedules[index] : Schedules.FirstOrDefault();
            UpdateScheduleDurations();
            if (SelectedCell?.Id > 0)
                WeakReferenceMessenger.Default.Send(new SchedulesUpdatedMessage(SelectedCell.Id));
        }

        /// <summary>Normalizes loop markers and validates that the end follows the start.</summary>
        private bool TryGetNormalizedLoopBounds(out int normalizedStartIndex, out int normalizedEndIndex)
        {
            var loopStart = -1;
            var loopEnd = -1;
            for (var i = 0; i < Sequence.Count; i++)
            {
                if (loopStart < 0 && Sequence[i].Kind == StepKind.LoopStart)
                    loopStart = i;
                if (loopEnd < 0 && Sequence[i].Kind == StepKind.LoopEnd)
                    loopEnd = i;
                if (loopStart >= 0 && loopEnd >= 0)
                    break;
            }

            if (loopStart < 0 || loopEnd < 0)
            {
                normalizedStartIndex = 0;
                normalizedEndIndex = 0;
                UpdateLoopSummary(loopStart + 1, loopEnd + 1, true);
                return true;
            }

            if (loopEnd <= loopStart)
            {
                normalizedStartIndex = 0;
                normalizedEndIndex = 0;
                UpdateLoopSummary(loopStart + 1, loopEnd + 1, false);
                return false;
            }

            normalizedStartIndex = loopStart + 1;
            normalizedEndIndex = loopEnd + 1;
            UpdateLoopSummary(normalizedStartIndex, normalizedEndIndex, true);
            return true;
        }

        partial void OnLoopStartIndexChanged(int value)
        {
            SaveScheduleCommand?.NotifyCanExecuteChanged();
            RefreshLoopSummary();
            OnPropertyChanged(nameof(CanSaveSchedule));
            OnPropertyChanged(nameof(LoopRangeDisplay));
        }

        partial void OnLoopEndIndexChanged(int value)
        {
            SaveScheduleCommand?.NotifyCanExecuteChanged();
            RefreshLoopSummary();
            OnPropertyChanged(nameof(CanSaveSchedule));
            OnPropertyChanged(nameof(LoopRangeDisplay));
        }

        partial void OnRepeatCountChanged(int value)
        {
            UpdateTotalDuration();
        }

        partial void OnScheduleStartDateChanged(DateTime value)
        {
            _scheduleStartDate = value.Date;
            UpdateScheduleSummaryText();
            RefreshCalendarSchedule();
            OnPropertyChanged(nameof(ScheduleStartDateTime));
            OnPropertyChanged(nameof(ScheduleEndDateTime));
        }

        partial void OnScheduleStartTimeChanged(TimeSpan value)
        {
            UpdateScheduleSummaryText();
            RefreshCalendarSchedule();
            OnPropertyChanged(nameof(ScheduleStartDateTime));
            OnPropertyChanged(nameof(ScheduleEndDateTime));
            OnPropertyChanged(nameof(ScheduleStartTimePickerValue));
        }

        partial void OnTotalDurationChanged(TimeSpan value)
        {
            OnPropertyChanged(nameof(ScheduleEndDateTime));
        }

        private void UpdateScheduleSummaryText()
        {
            var profileCount = Sequence.Count(s => s.Kind == StepKind.Profile);
            var repeatCount = Math.Max(1, RepeatCount);
            var durationText = TotalDuration == TimeSpan.Zero
                ? "00:00:00"
                : TotalDuration.ToString("hh\\:mm\\:ss");

            var startLabel = ScheduleStartDateTime.ToString("yyyy-MM-dd HH:mm");
            var endLabel = ScheduleEndDateTime.ToString("yyyy-MM-dd HH:mm");

            var stepSummary = profileCount switch
            {
                0 => "No profile steps configured",
                1 => "1 profile step configured",
                _ => $"{profileCount} profile steps configured"
            };

            ScheduleSummaryText = $"{stepSummary} • Total duration: {durationText} • Repeat count: {repeatCount} • {startLabel} → {endLabel}";
        }

        private void UpdateLoopSummary(int displayStartIndex, int displayEndIndex, bool isValid)
        {
            var hasLoop = displayStartIndex > 0 && displayEndIndex > 0;
            var repeatCount = Math.Max(1, RepeatCount);

            string summary;
            if (!hasLoop)
            {
                summary = repeatCount > 1
                    ? $"Loop markers missing • Repeat count ignored ({repeatCount})"
                    : "Loop markers not configured";
            }
            else if (!isValid)
            {
                summary = $"Loop start {displayStartIndex} must precede end {displayEndIndex}";
            }
            else
            {
                var repeatText = repeatCount > 1 ? $"repeats {repeatCount} times" : "no repetition";
                summary = $"Loop range: {displayStartIndex} → {displayEndIndex} • {repeatText}";
            }

            IsLoopValid = isValid;
            LoopSummaryText = summary;
            OnPropertyChanged(nameof(LoopRangeDisplay));
        }

        private void RefreshLoopSummary()
        {
            TryGetNormalizedLoopBounds(out _, out _);
        }

        private void RefreshCalendarSchedule()
        {
            CalendarDays.Clear();

            var steps = BuildExecutionPlan().ToList();
            if (!steps.Any())
                return;

            var current = ScheduleStartDateTime;
            var dayMap = new Dictionary<DateTime, ScheduleCalendarDay>();

            foreach (var step in steps)
            {
                var start = current;
                var end = current + step.Duration;
                var entry = new ScheduleCalendarEntry(
                    step.Order,
                    step.Name,
                    start,
                    end,
                    step.Duration,
                    step.IsLoopSegment,
                    step.LoopIteration);

                var key = start.Date;
                if (!dayMap.TryGetValue(key, out var day))
                {
                    day = new ScheduleCalendarDay(key);
                    dayMap.Add(key, day);
                }

                day.Entries.Add(entry);
                current = end;
            }

            foreach (var day in dayMap.Values.OrderBy(d => d.Date))
                CalendarDays.Add(day);
        }

        private IEnumerable<ExecutionStep> BuildExecutionPlan()
        {
            var results = new List<ExecutionStep>();
            if (!Sequence.Any())
                return results;

            var loopStartIndex = Sequence.ToList().FindIndex(s => s.Kind == StepKind.LoopStart);
            var loopEndIndex = Sequence.ToList().FindIndex(s => s.Kind == StepKind.LoopEnd);
            var hasValidLoop = loopStartIndex >= 0 && loopEndIndex > loopStartIndex;

            var beforeLoop = new List<StepTemplate>();
            var loopSteps = new List<StepTemplate>();
            var afterLoop = new List<StepTemplate>();

            var encounteredLoopStart = false;
            var encounteredLoopEnd = false;
            var insideLoop = false;

            foreach (var step in Sequence)
            {
                if (step.Kind == StepKind.LoopStart)
                {
                    encounteredLoopStart = true;
                    insideLoop = true;
                    continue;
                }

                if (step.Kind == StepKind.LoopEnd)
                {
                    encounteredLoopEnd = true;
                    insideLoop = false;
                    continue;
                }

                if (step.Kind != StepKind.Profile)
                    continue;

                if (!encounteredLoopStart)
                {
                    beforeLoop.Add(step);
                }
                else if (insideLoop || !encounteredLoopEnd)
                {
                    loopSteps.Add(step);
                }
                else
                {
                    afterLoop.Add(step);
                }
            }

            var order = 1;

            foreach (var step in beforeLoop)
                results.Add(new ExecutionStep(order++, step.Name, step.Duration, false, 0));

            if (loopSteps.Any())
            {
                var iterations = hasValidLoop ? Math.Max(1, RepeatCount) : 1;
                for (var iteration = 1; iteration <= iterations; iteration++)
                {
                    foreach (var step in loopSteps)
                        results.Add(new ExecutionStep(order++, step.Name, step.Duration, hasValidLoop, hasValidLoop ? iteration : 0));
                }
            }

            foreach (var step in afterLoop)
                results.Add(new ExecutionStep(order++, step.Name, step.Duration, false, 0));

            return results;
        }

        private sealed class ExecutionStep
        {
            public ExecutionStep(int order, string name, TimeSpan duration, bool isLoopSegment, int loopIteration)
            {
                Order = order;
                Name = name;
                Duration = duration;
                IsLoopSegment = isLoopSegment;
                LoopIteration = loopIteration;
            }

            public int Order { get; }
            public string Name { get; }
            public TimeSpan Duration { get; }
            public bool IsLoopSegment { get; }
            public int LoopIteration { get; }
        }
    }

    /// <summary>Represents a single day within the schedule calendar preview.</summary>
    public class ScheduleCalendarDay
    {
        public ScheduleCalendarDay(DateTime date)
        {
            Date = date.Date;
        }

        public DateTime Date { get; }

        public string Header => Date.ToString("yyyy-MM-dd (ddd)");

        public ObservableCollection<ScheduleCalendarEntry> Entries { get; } = new();

        public string TotalDurationText
        {
            get
            {
                if (!Entries.Any())
                    return "Total for day: 00:00:00";
                var ticks = Entries.Sum(e => e.Duration.Ticks);
                return $"Total for day: {new TimeSpan(ticks):hh\\:mm\\:ss}";
            }
        }
    }

    /// <summary>Describes a scheduled step with concrete start and end timestamps.</summary>
    public class ScheduleCalendarEntry
    {
        public ScheduleCalendarEntry(int order, string stepName, DateTime start, DateTime end, TimeSpan duration, bool isLoopSegment, int loopIteration)
        {
            Order = order;
            StepName = stepName;
            Start = start;
            End = end;
            Duration = duration;
            IsLoopSegment = isLoopSegment;
            LoopIteration = loopIteration;
        }

        public int Order { get; }
        public string StepName { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public TimeSpan Duration { get; }
        public bool IsLoopSegment { get; }
        public int LoopIteration { get; }

        public bool HasLoopIteration => IsLoopSegment && LoopIteration > 0;

        public string StepLabel => $"#{Order} {StepName}";

        public string StartDisplay => $"Start: {Start:yyyy-MM-dd HH:mm}";

        public string EndDisplay => $"End: {End:yyyy-MM-dd HH:mm}";

        public string DurationDisplay => $"Duration: {Duration:hh\\:mm\\:ss}";

        public string TimeRangeDisplay => $"{Start:HH:mm} – {End:HH:mm}";

        public string DurationCompactDisplay
        {
            get
            {
                if (Duration == TimeSpan.Zero)
                    return "0m";

                var parts = new List<string>();

                if (Duration.TotalHours >= 1)
                    parts.Add($"{(int)Duration.TotalHours}h");

                if (Duration.Minutes > 0)
                    parts.Add($"{Duration.Minutes}m");

                if (parts.Count == 0 && Duration.Seconds > 0)
                    parts.Add($"{Duration.Seconds}s");

                return string.Join(" ", parts);
            }
        }

        public string LoopIterationDisplay => HasLoopIteration ? $"Loop iteration {LoopIteration}" : string.Empty;

        public string LoopBadgeText => HasLoopIteration ? $"Loop #{LoopIteration}" : string.Empty;
    }
}
