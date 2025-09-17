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
            };

            RemoveStepCommand = new RelayCommand<StepTemplate>(s => Sequence.Remove(s));
            SaveScheduleCommand = new RelayCommand(SaveSchedule);
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
            TotalDuration = new TimeSpan(Sequence.Sum(s => s.Duration.Ticks));
            if (SelectedSchedule != null)
                SelectedSchedule.EstimatedDuration = TotalDuration;
            UpdateLoopIndices();
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
                var ticks = sched.TestProfileIds
                    .Select(id => StepLibrary.SelectMany(g => g.Steps)
                        .FirstOrDefault(s => s.Id == id)?.Duration.Ticks ?? 0)
                    .Sum();
                sched.EstimatedDuration = new TimeSpan(ticks);
            }
        }

        /// <summary>Updates cached indices for loop start and end markers in the sequence.</summary>
        private void UpdateLoopIndices()
        {
            LoopStartIndex = Sequence.IndexOf(Sequence.FirstOrDefault(s => s.Kind == StepKind.LoopStart)) + 1;
            LoopEndIndex = Sequence.IndexOf(Sequence.FirstOrDefault(s => s.Kind == StepKind.LoopEnd)) + 1;
        }

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
            DeleteScheduleCommand.NotifyCanExecuteChanged();
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
        }

        /// <summary>Persists the current schedule to the repository and notifies listeners.</summary>
        private void SaveSchedule()
        {
            if (SelectedSchedule == null) return;
            SelectedSchedule.Name = ScheduleName;
            SelectedSchedule.TestProfileIds = Sequence.Where(s => s.Kind == StepKind.Profile).Select(s => s.Id).ToList();
            SelectedSchedule.RepeatCount = RepeatCount;
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
    }
}
