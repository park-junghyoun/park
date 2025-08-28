using System.Collections.ObjectModel;
using System.Linq;
using CellManager.Models;
using CellManager.Models.TestProfile;
using CellManager.Messages;
using CellManager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CellManager.ViewModels
{
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
        public ObservableCollection<ProfileReference> WorkingSchedule { get; } = new();

        [ObservableProperty]
        private Cell? _selectedCell;

        [ObservableProperty]
        private string _scheduleName = "New Schedule";

        [ObservableProperty]
        private string? _notes;

        public RelayCommand SaveScheduleCommand { get; }

        public RelayCommand<ProfileReference> RemoveProfileCommand { get; }
        public RelayCommand ClearScheduleCommand { get; }

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
            RemoveProfileCommand = new RelayCommand<ProfileReference>(p => WorkingSchedule.Remove(p));
            ClearScheduleCommand = new RelayCommand(ClearSchedule, () => WorkingSchedule.Count > 0);

            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                SelectedCell = m.SelectedCell;
                LoadProfiles();
            });

            LoadProfiles();
        }

        private void LoadProfiles()
        {
            ProfileLibrary.Clear();
            if (SelectedCell?.Id > 0)
            {
                foreach (var p in _chargeProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { Type = TestProfileType.Charge, Id = p.Id, Name = p.Name });
                foreach (var p in _dischargeProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { Type = TestProfileType.Discharge, Id = p.Id, Name = p.Name });
                foreach (var p in _ecmPulseProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { Type = TestProfileType.ECM, Id = p.Id, Name = p.Name });
                foreach (var p in _ocvProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { Type = TestProfileType.OCV, Id = p.Id, Name = p.Name });
                foreach (var p in _restProfileRepository.Load(SelectedCell.Id))
                    ProfileLibrary.Add(new ProfileReference { Type = TestProfileType.Rest, Id = p.Id, Name = p.Name });
            }
        }

        public void InsertProfile(ProfileReference profile, int index)
        {
            if (WorkingSchedule.Contains(profile))
            {
                var oldIndex = WorkingSchedule.IndexOf(profile);
                if (oldIndex < index) index--;
                WorkingSchedule.RemoveAt(oldIndex);
            }
            if (index < 0 || index > WorkingSchedule.Count)
                WorkingSchedule.Add(profile);
            else
                WorkingSchedule.Insert(index, profile);

            SaveScheduleCommand.NotifyCanExecuteChanged();
            ClearScheduleCommand.NotifyCanExecuteChanged();
        }

        private void SaveSchedule()
        {
            var schedule = new Schedule
            {
                Name = ScheduleName,
                Notes = Notes,
                TestProfileIds = WorkingSchedule.Select(p => p.Id).ToList(),
                Ordering = 0
            };
            _scheduleRepository.Save(schedule);
        }

        private void RemoveProfile(ProfileReference profile)
        {
            WorkingSchedule.Remove(profile);
            SaveScheduleCommand.NotifyCanExecuteChanged();
            ClearScheduleCommand.NotifyCanExecuteChanged();
        }

        private void ClearSchedule()
        {
            WorkingSchedule.Clear();
            SaveScheduleCommand.NotifyCanExecuteChanged();
            ClearScheduleCommand.NotifyCanExecuteChanged();
        }
    }
}