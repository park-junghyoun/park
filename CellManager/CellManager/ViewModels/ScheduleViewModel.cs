using System.Collections.ObjectModel;
using System.Linq;
using CellManager.Models;
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

        private readonly ITestProfileRepository _testProfileRepository;
        private readonly IScheduleRepository _scheduleRepository;

        public ObservableCollection<TestProfileModel> ProfileLibrary { get; } = new();
        public ObservableCollection<TestProfileModel> WorkingSchedule { get; } = new();

        [ObservableProperty]
        private Cell _selectedCell;

        [ObservableProperty]
        private string _scheduleName = "New Schedule";

        [ObservableProperty]
        private string? _notes;

        public RelayCommand SaveScheduleCommand { get; }
        public RelayCommand<TestProfileModel> RemoveProfileCommand { get; }

        public ScheduleViewModel(ITestProfileRepository testProfileRepository, IScheduleRepository scheduleRepository)
        {
            _testProfileRepository = testProfileRepository;
            _scheduleRepository = scheduleRepository;

            SaveScheduleCommand = new RelayCommand(SaveSchedule, () => WorkingSchedule.Count > 0);
            RemoveProfileCommand = new RelayCommand<TestProfileModel>(p => WorkingSchedule.Remove(p));

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
                var profiles = _testProfileRepository.LoadTestProfiles(SelectedCell.Id);
                foreach (var p in profiles)
                    ProfileLibrary.Add(p);
            }
        }

        public void InsertProfile(TestProfileModel profile, int index)
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
    }
}