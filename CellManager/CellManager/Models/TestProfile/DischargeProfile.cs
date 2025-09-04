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
                return $"Mode: {DischargeMode}, Current: {DischargeCurrent} A, Cutoff: {DischargeCutoffVoltage} V, {modeText}";
            }
        }
    }
}
