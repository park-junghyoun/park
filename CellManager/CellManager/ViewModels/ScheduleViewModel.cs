using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CellManager.Models;

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

        public RelayCommand<StepTemplate> RemoveStepCommand { get; }
        public RelayCommand SaveScheduleCommand { get; }
        public RelayCommand AddScheduleCommand { get; }

        public ScheduleViewModel()
        {
            BuildMockLibrary();
            Sequence.CollectionChanged += (_, __) => UpdateTotalDuration();

            RemoveStepCommand = new RelayCommand<StepTemplate>(s => Sequence.Remove(s));
            SaveScheduleCommand = new RelayCommand(SaveSchedule);
            AddScheduleCommand = new RelayCommand(AddSchedule);

            UpdateTotalDuration();
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

            Schedules.Add(new Schedule { Id = 1, Name = "Schedule A", TestProfileIds = { 1, 2 } });
            Schedules.Add(new Schedule { Id = 2, Name = "Schedule B", TestProfileIds = { 3 } });
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
