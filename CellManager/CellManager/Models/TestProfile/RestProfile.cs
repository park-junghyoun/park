using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    /// <summary>
    ///     Captures the timing information for rest periods between charge/discharge steps.
    /// </summary>
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

        /// <summary>Displays the rest duration in a user-friendly format.</summary>
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

        /// <summary>
        ///     Combines the individual hour, minute and second fields into the backing <see cref="TimeSpan"/>.
        /// </summary>
        private void UpdateRestTime()
        {
            RestTime = new TimeSpan(RestHours, RestMinutes, RestSeconds);
        }
    }
}
