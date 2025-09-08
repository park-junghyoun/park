using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CellManager.Models.TestProfile
{
    public enum DischargeMode
    {
        DischargeByCapacity,
        DischargeByTime,
        FullDischarge
    }

    public partial class DischargeProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private DischargeMode _dischargeMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _dischargeCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _dischargeCutoffVoltage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private TimeSpan _dischargeTime;

        [ObservableProperty]
        private int _dischargeHours;

        [ObservableProperty]
        private int _dischargeMinutes;

        [ObservableProperty]
        private int _dischargeSeconds;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double? _dischargeCapacityMah;

        public string PreviewText
        {
            get
            {
                var modeText = DischargeMode switch
                {
                    DischargeMode.DischargeByCapacity => $"Capacity: {DischargeCapacityMah} mAh",
                    DischargeMode.DischargeByTime => $"Time: {DischargeTime}",
                    _ => "Full discharge"
                };
                return $"Mode: {DischargeMode}, Current: {DischargeCurrent} mA, Cutoff: {DischargeCutoffVoltage} mV";
            }
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

        private void UpdateDischargeTime()
        {
            DischargeTime = new TimeSpan(DischargeHours, DischargeMinutes, DischargeSeconds);
        }
    }
}
