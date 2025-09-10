using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class RestProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private TimeSpan _restTime;

        [ObservableProperty] private int _restHours;
        [ObservableProperty] private int _restMinutes;
        [ObservableProperty] private int _restSeconds;

        public string PreviewText => $"{RestTime}";

        partial void OnRestHoursChanged(int value) => UpdateRestTime();
        partial void OnRestMinutesChanged(int value) => UpdateRestTime();
        partial void OnRestSecondsChanged(int value) => UpdateRestTime();

        partial void OnRestTimeChanged(TimeSpan value)
        {
            SetProperty(ref _restHours, value.Hours, nameof(RestHours));
            SetProperty(ref _restMinutes, value.Minutes, nameof(RestMinutes));
            SetProperty(ref _restSeconds, value.Seconds, nameof(RestSeconds));
        }

        private void UpdateRestTime()
        {
            RestTime = new TimeSpan(RestHours, RestMinutes, RestSeconds);
        }
    }
}
