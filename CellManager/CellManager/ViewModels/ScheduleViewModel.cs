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
    public class StepTemplate : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IconKind { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
    }

    public class StepGroup : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string IconKind { get; set; } = string.Empty;
        public ObservableCollection<StepTemplate> Steps { get; } = new();
    }

    public partial class ScheduleViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Schedule";
        public string IconName { get; } = "Calendar";

        private readonly IChargeProfileRepository? _chargeRepo;
        private readonly IDischargeProfileRepository? _dischargeRepo;
        private readonly IRestProfileRepository? _restRepo;
        private readonly IOcvProfileRepository? _ocvRepo;
        private readonly IEcmPulseProfileRepository? _ecmRepo;

        [ObservableProperty]
        private bool _isViewEnabled = true;

        public ObservableCollection<StepGroup> StepLibrary { get; } = new();
        public ObservableCollection<StepTemplate> Sequence { get; } = new();
        public ObservableCollection<Schedule> Schedules { get; } = new();

        [ObservableProperty] private Schedule? _selectedSchedule;

        [ObservableProperty] private string _scheduleName = "New Schedule";
        [ObservableProperty] private int _repeatCount = 1;
        [ObservableProperty] private int _loopStartIndex;
        [ObservableProperty] private TimeSpan _totalDuration;
        [ObservableProperty] private Cell? _selectedCell;

        public RelayCommand<StepTemplate> RemoveStepCommand { get; }
        public RelayCommand SaveScheduleCommand { get; }
        public RelayCommand AddScheduleCommand { get; }

        public ScheduleViewModel() : this(null, null, null, null, null) { }

        public ScheduleViewModel(
            IChargeProfileRepository? chargeRepo,
            IDischargeProfileRepository? dischargeRepo,
            IEcmPulseProfileRepository? ecmRepo,
            IOcvProfileRepository? ocvRepo,
            IRestProfileRepository? restRepo)
        {
            _chargeRepo = chargeRepo;
            _dischargeRepo = dischargeRepo;
            _restRepo = restRepo;
            _ocvRepo = ocvRepo;
            _ecmRepo = ecmRepo;

            Sequence.CollectionChanged += (_, __) => UpdateTotalDuration();

            RemoveStepCommand = new RelayCommand<StepTemplate>(s => Sequence.Remove(s));
            SaveScheduleCommand = new RelayCommand(SaveSchedule);
            AddScheduleCommand = new RelayCommand(AddSchedule);

            if (_chargeRepo == null)
            {
                BuildMockLibrary();
            }
            else
            {
                BuildMockSchedules();
                WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
                {
                    SelectedCell = m.SelectedCell;
                    LoadStepLibrary();
                });
            }

            UpdateTotalDuration();
        }

        private void BuildMockSchedules()
        {
            Schedules.Add(new Schedule { Id = 1, Name = "Schedule A", TestProfileIds = { 1, 2 } });
            Schedules.Add(new Schedule { Id = 2, Name = "Schedule B", TestProfileIds = { 3 } });
        }

        private void BuildMockLibrary()
        {
            var id = 1;
            StepLibrary.Add(new StepGroup
            {
                Name = "Charge",
                IconKind = "Battery",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "Charge",
                        IconKind = "Battery",
                        Parameters = "0.5A → 4.2V | 01:00:00",
                        Duration = TimeSpan.FromHours(1)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "Discharge",
                IconKind = "ArrowDown",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "Discharge",
                        IconKind = "ArrowDown",
                        Parameters = "0.5A → 3.0V | 00:30:00",
                        Duration = TimeSpan.FromMinutes(30)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "Rest",
                IconKind = "Pause",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "Rest",
                        IconKind = "Pause",
                        Parameters = "00:10:00",
                        Duration = TimeSpan.FromMinutes(10)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "OCV",
                IconKind = "ChartBar",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "OCV",
                        IconKind = "ChartBar",
                        Parameters = "01:00:00",
                        Duration = TimeSpan.FromHours(1)
                    }
                }
            });

            StepLibrary.Add(new StepGroup
            {
                Name = "ECM",
                IconKind = "Wrench",
                Steps =
                {
                    new StepTemplate
                    {
                        Id = id++,
                        Name = "ECM",
                        IconKind = "Wrench",
                        Parameters = "0.2A 00:05:00",
                        Duration = TimeSpan.FromMinutes(5)
                    }
                }
            });

            BuildMockSchedules();
        }

        private void LoadStepLibrary()
        {
            StepLibrary.Clear();
            if (SelectedCell == null) return;

            if (_chargeRepo != null)
            {
                var chargeGroup = new StepGroup { Name = "Charge", IconKind = "Battery" };
                foreach (var p in _chargeRepo.Load(SelectedCell.Id))
                {
                    chargeGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "Battery",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.Charge, p) ?? TimeSpan.Zero
                    });
                }
                if (chargeGroup.Steps.Any()) StepLibrary.Add(chargeGroup);
            }

            if (_dischargeRepo != null)
            {
                var disGroup = new StepGroup { Name = "Discharge", IconKind = "ArrowDown" };
                foreach (var p in _dischargeRepo.Load(SelectedCell.Id))
                {
                    disGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "ArrowDown",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.Discharge, p) ?? TimeSpan.Zero
                    });
                }
                if (disGroup.Steps.Any()) StepLibrary.Add(disGroup);
            }

            if (_restRepo != null)
            {
                var restGroup = new StepGroup { Name = "Rest", IconKind = "Pause" };
                foreach (var p in _restRepo.Load(SelectedCell.Id))
                {
                    restGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "Pause",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.Rest, p) ?? TimeSpan.Zero
                    });
                }
                if (restGroup.Steps.Any()) StepLibrary.Add(restGroup);
            }

            if (_ocvRepo != null)
            {
                var ocvGroup = new StepGroup { Name = "OCV", IconKind = "ChartBar" };
                foreach (var p in _ocvRepo.Load(SelectedCell.Id))
                {
                    ocvGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "ChartBar",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.OCV, p) ?? TimeSpan.Zero
                    });
                }
                if (ocvGroup.Steps.Any()) StepLibrary.Add(ocvGroup);
            }

            if (_ecmRepo != null)
            {
                var ecmGroup = new StepGroup { Name = "ECM", IconKind = "Wrench" };
                foreach (var p in _ecmRepo.Load(SelectedCell.Id))
                {
                    ecmGroup.Steps.Add(new StepTemplate
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IconKind = "Wrench",
                        Parameters = p.PreviewText,
                        Duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, TestProfileType.ECM, p) ?? TimeSpan.Zero
                    });
                }
                if (ecmGroup.Steps.Any()) StepLibrary.Add(ecmGroup);
            }

            if (SelectedSchedule != null)
                OnSelectedScheduleChanged(SelectedSchedule);
        }

        public void InsertStep(StepTemplate template, int index)
        {
            var clone = new StepTemplate
            {
                Id = template.Id,
                Name = template.Name,
                IconKind = template.IconKind,
                Parameters = template.Parameters,
                Duration = template.Duration
            };
            if (index < 0 || index > Sequence.Count)
                Sequence.Add(clone);
            else
                Sequence.Insert(index, clone);
        }

        public void MoveStep(StepTemplate step, int index)
        {
            var oldIndex = Sequence.IndexOf(step);
            if (oldIndex < 0) return;
            if (oldIndex < index) index--;
            if (index < 0) index = 0;
            if (index >= Sequence.Count) index = Sequence.Count - 1;
            Sequence.Move(oldIndex, index);
        }

        private void UpdateTotalDuration()
        {
            TotalDuration = new TimeSpan(Sequence.Sum(s => s.Duration.Ticks));
        }

        partial void OnSelectedScheduleChanged(Schedule? value)
        {
            Sequence.Clear();
            if (value == null) return;
            ScheduleName = value.Name;
            foreach (var id in value.TestProfileIds)
            {
                var template = StepLibrary.SelectMany(g => g.Steps).FirstOrDefault(s => s.Id == id);
                if (template != null)
                    InsertStep(template, -1);
            }
        }

        private void AddSchedule()
        {
            var newId = Schedules.Any() ? Schedules.Max(s => s.Id) + 1 : 1;
            var sched = new Schedule { Id = newId, Name = $"Schedule {newId}" };
            Schedules.Add(sched);
            SelectedSchedule = sched;
            Sequence.Clear();
            ScheduleName = sched.Name;
        }

        private void SaveSchedule()
        {
            if (SelectedSchedule == null) return;
            SelectedSchedule.Name = ScheduleName;
            SelectedSchedule.TestProfileIds = Sequence.Select(s => s.Id).ToList();
        }
    }
}
