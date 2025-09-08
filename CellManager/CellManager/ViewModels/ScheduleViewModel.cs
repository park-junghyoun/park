using System;
using System.Collections.ObjectModel;
using System.Linq;
using CellManager.Models;
using CellManager.Models.TestProfile;
using CellManager.Messages;
using CellManager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;

namespace CellManager.ViewModels
{
    public partial class ScheduledProfile : ObservableObject
    {
        public ProfileReference Reference { get; set; } = null!;

        [ObservableProperty]
        private TimeSpan? _estimatedDuration;

        [ObservableProperty]
        private DateTime? _estimatedStartTime;

        [ObservableProperty]
        private DateTime? _estimatedEndTime;

        public int UniqueId => Reference.UniqueId;
        public string DisplayNameAndId => Reference.DisplayNameAndId;
    }

    public partial class ScheduleViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Schedule";
        public string IconName { get; } = "Calendar";

        [ObservableProperty]
        private bool _isViewEnabled = true;

        private readonly IScheduleRepository _scheduleRepository;
        private readonly IChargeProfileRepository _chargeProfileRepository;
        private readonly IDischargeProfileRepository _dischargeProfileRepository;
        private readonly IEcmPulseProfileRepository _ecmPulseProfileRepository;
        private readonly IOcvProfileRepository _ocvProfileRepository;
        private readonly IRestProfileRepository _restProfileRepository;

        public ObservableCollection<ProfileReference> ProfileLibrary { get; } = new();
        public ObservableCollection<ScheduledProfile> WorkingSchedule { get; } = new();

        public ObservableCollection<Schedule> Schedules { get; } = new();

        [ObservableProperty]
        private Cell? _selectedCell;

        [ObservableProperty]
        private string _scheduleName = "New Schedule";

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private Schedule? _selectedSchedule;

        public RelayCommand SaveScheduleCommand { get; }

        public RelayCommand<ScheduledProfile> RemoveProfileCommand { get; }
        public RelayCommand ClearScheduleCommand { get; }
        public RelayCommand NewScheduleCommand { get; }
        public RelayCommand DeleteScheduleCommand { get; }

        public ScheduleViewModel(
            IChargeProfileRepository chargeProfileRepository,
            IDischargeProfileRepository dischargeProfileRepository,
            IEcmPulseProfileRepository ecmPulseProfileRepository,
            IOcvProfileRepository ocvProfileRepository,
            IRestProfileRepository restProfileRepository,
            IScheduleRepository scheduleRepository)
        {
            _chargeProfileRepository = chargeProfileRepository;
            _dischargeProfileRepository = dischargeProfileRepository;
            _ecmPulseProfileRepository = ecmPulseProfileRepository;
            _ocvProfileRepository = ocvProfileRepository;
            _restProfileRepository = restProfileRepository;
            _scheduleRepository = scheduleRepository;

            SaveScheduleCommand = new RelayCommand(SaveSchedule, () => WorkingSchedule.Count > 0);
            RemoveProfileCommand = new RelayCommand<ScheduledProfile>(RemoveProfile);
            ClearScheduleCommand = new RelayCommand(ClearSchedule, () => WorkingSchedule.Count > 0);
            NewScheduleCommand = new RelayCommand(NewSchedule);
            DeleteScheduleCommand = new RelayCommand(DeleteSchedule, () => SelectedSchedule != null);

            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                SelectedCell = m.SelectedCell;
                LoadProfiles();
                RecalculateScheduleTimes();
            });

            LoadProfiles();
            LoadSchedules();
        }

        private void LoadSchedules()
        {
            Schedules.Clear();
            foreach (var schedule in _scheduleRepository.GetAll())
                Schedules.Add(schedule);
        }

        private void LoadProfiles()
        {
            ProfileLibrary.Clear();
            if (SelectedCell?.Id > 0)
            {
                foreach (var p in _chargeProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { CellId = SelectedCell.Id, Type = TestProfileType.Charge, Id = p.Id, Name = p.Name });
                foreach (var p in _dischargeProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { CellId = SelectedCell.Id, Type = TestProfileType.Discharge, Id = p.Id, Name = p.Name });
                foreach (var p in _ecmPulseProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { CellId = SelectedCell.Id, Type = TestProfileType.ECM, Id = p.Id, Name = p.Name });
                foreach (var p in _ocvProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { CellId = SelectedCell.Id, Type = TestProfileType.OCV, Id = p.Id, Name = p.Name });
                foreach (var p in _restProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { CellId = SelectedCell.Id, Type = TestProfileType.Rest, Id = p.Id, Name = p.Name });
            }
        }

        public void InsertProfile(ProfileReference profile, int index)
        {
            var existing = WorkingSchedule.FirstOrDefault(p => p.Reference.UniqueId == profile.UniqueId);
            if (existing != null)
            {
                var oldIndex = WorkingSchedule.IndexOf(existing);
                if (oldIndex < index) index--;
                WorkingSchedule.RemoveAt(oldIndex);
            }

            var item = new ScheduledProfile { Reference = profile };
            if (index < 0 || index > WorkingSchedule.Count)
                WorkingSchedule.Add(item);
            else
                WorkingSchedule.Insert(index, item);

            RecalculateScheduleTimes();
            SaveScheduleCommand.NotifyCanExecuteChanged();
            ClearScheduleCommand.NotifyCanExecuteChanged();
        }

        private void SaveSchedule()
        {
            var duplicates = _scheduleRepository.GetAll().Any(s => s.Name == ScheduleName);
            if (duplicates)
            {
                MessageBox.Show(
                    "A schedule with this name already exists.",
                    "Duplicate Schedule",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var schedule = new Schedule
            {
                Name = ScheduleName,
                Notes = Notes,
                TestProfileIds = WorkingSchedule.Select(p => p.Reference.UniqueId).ToList(),
                Ordering = 0
            };
            _scheduleRepository.Save(schedule);

            if (!Schedules.Contains(schedule))
                Schedules.Add(schedule);

            SelectedSchedule = schedule;
        }

        private void RemoveProfile(ScheduledProfile profile)
        {
            WorkingSchedule.Remove(profile);
            RecalculateScheduleTimes();
            SaveScheduleCommand.NotifyCanExecuteChanged();
            ClearScheduleCommand.NotifyCanExecuteChanged();
        }

        private void ClearSchedule()
        {
            WorkingSchedule.Clear();
            RecalculateScheduleTimes();
            SaveScheduleCommand.NotifyCanExecuteChanged();
            ClearScheduleCommand.NotifyCanExecuteChanged();
        }

        private void NewSchedule()
        {
            SelectedSchedule = null;
            ScheduleName = "New Schedule";
            Notes = null;
            ClearSchedule();
        }

        private void DeleteSchedule()
        {
            if (SelectedSchedule == null) return;
            _scheduleRepository.Delete(SelectedSchedule.Id);
            Schedules.Remove(SelectedSchedule);
            NewSchedule();
        }

        partial void OnSelectedScheduleChanged(Schedule? value)
        {
            if (value != null)
            {
                ScheduleName = value.Name;
                Notes = value.Notes;
                WorkingSchedule.Clear();
                foreach (var id in value.TestProfileIds)
                {
                    var profile = ProfileLibrary.FirstOrDefault(p => p.UniqueId == id);
                    if (profile != null)
                        WorkingSchedule.Add(new ScheduledProfile { Reference = profile });
                }
            }
            else
            {
                ScheduleName = "New Schedule";
                Notes = null;
                WorkingSchedule.Clear();
            }

            RecalculateScheduleTimes();
            SaveScheduleCommand.NotifyCanExecuteChanged();
            ClearScheduleCommand.NotifyCanExecuteChanged();
            DeleteScheduleCommand.NotifyCanExecuteChanged();
        }

        private object? LoadProfile(ProfileReference reference) => reference.Type switch
        {
            TestProfileType.Charge => _chargeProfileRepository.Load(reference.CellId).FirstOrDefault(p => p.Id == reference.Id),
            TestProfileType.Discharge => _dischargeProfileRepository.Load(reference.CellId).FirstOrDefault(p => p.Id == reference.Id),
            TestProfileType.ECM => _ecmPulseProfileRepository.Load(reference.CellId).FirstOrDefault(p => p.Id == reference.Id),
            TestProfileType.OCV => _ocvProfileRepository.Load(reference.CellId).FirstOrDefault(p => p.Id == reference.Id),
            TestProfileType.Rest => _restProfileRepository.Load(reference.CellId).FirstOrDefault(p => p.Id == reference.Id),
            _ => null
        };

        private void RecalculateScheduleTimes()
        {
            if (SelectedCell == null)
            {
                foreach (var item in WorkingSchedule)
                {
                    item.EstimatedDuration = null;
                    item.EstimatedStartTime = null;
                    item.EstimatedEndTime = null;
                }
                return;
            }

            var current = DateTime.Now;
            foreach (var item in WorkingSchedule)
            {
                item.EstimatedStartTime = current;
                var profile = LoadProfile(item.Reference);
                var duration = ScheduleTimeCalculator.EstimateDuration(SelectedCell, item.Reference.Type, profile);
                item.EstimatedDuration = duration;
                if (duration.HasValue)
                {
                    item.EstimatedEndTime = current + duration.Value;
                    current = item.EstimatedEndTime.Value;
                }
                else
                {
                    item.EstimatedEndTime = null;
                }
            }
        }
    }
}