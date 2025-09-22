using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CellManager.Models.TestProfile
{
    /// <summary>
    ///     Supported discharge termination strategies for a test profile.
    /// </summary>
    public enum DischargeMode
    {
        DischargeByCapacity,
        DischargeByTime,
        FullDischarge
    }

    /// <summary>
    ///     Represents the configuration for the discharge segment of a test profile, including
    ///     preview text helpers that summarize the selected settings.
    /// </summary>
    public partial class DischargeProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        [NotifyPropertyChangedFor(nameof(TestSetupPreviewText))]
        private DischargeMode _dischargeMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        [NotifyPropertyChangedFor(nameof(TestSetupPreviewText))]
        private double _dischargeCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        [NotifyPropertyChangedFor(nameof(TestSetupPreviewText))]
        private double _dischargeCutoffVoltage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        [NotifyPropertyChangedFor(nameof(TestSetupPreviewText))]
        private TimeSpan _dischargeTime;

        [ObservableProperty]
        private int _dischargeHours;

        [ObservableProperty]
        private int _dischargeMinutes;

        [ObservableProperty]
        private int _dischargeSeconds;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        [NotifyPropertyChangedFor(nameof(TestSetupPreviewText))]
        private double? _dischargeCapacityMah;

        /// <summary>
        ///     User-facing summary of the discharge configuration shown in the profile tree.
        /// </summary>
        public string PreviewText => BuildPreviewText(includeDischargeMode: true);

        /// <summary>
        ///     Alternate preview used in the test setup view that hides the discharge mode portion.
        /// </summary>
        public string TestSetupPreviewText => BuildPreviewText(includeDischargeMode: false);

        private string BuildPreviewText(bool includeDischargeMode)
        {
            var baseText = includeDischargeMode
                ? $"{DischargeMode} , {DischargeCutoffVoltage} mV, {DischargeCurrent} mA"
                : $"{DischargeCutoffVoltage} mV, {DischargeCurrent} mA";

            return DischargeMode switch
            {
                DischargeMode.DischargeByCapacity => $"{baseText}, {DischargeCapacityMah} mAh",
                DischargeMode.DischargeByTime => $"{baseText}, {DischargeTime}",
                _ => baseText
            };
        }

        partial void OnDischargeHoursChanged(int value) => UpdateDischargeTime();
        partial void OnDischargeMinutesChanged(int value) => UpdateDischargeTime();
        partial void OnDischargeSecondsChanged(int value) => UpdateDischargeTime();

        partial void OnDischargeTimeChanged(TimeSpan value)
        {
            SetProperty(ref _dischargeHours, value.Hours, nameof(DischargeHours));
            SetProperty(ref _dischargeMinutes, value.Minutes, nameof(DischargeMinutes));
            SetProperty(ref _dischargeSeconds, value.Seconds, nameof(DischargeSeconds));
        }

        /// <summary>
        ///     Reconstructs the <see cref="TimeSpan"/> whenever one of the individual components changes.
        /// </summary>
        private void UpdateDischargeTime()
        {
            DischargeTime = new TimeSpan(DischargeHours, DischargeMinutes, DischargeSeconds);
        }
    }
}
