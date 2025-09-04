using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class ChargeProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _chargeCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _chargeCutoffVoltage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _cutoffCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private ChargeMode _chargeMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double? _chargeCapacityMah;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private TimeSpan _chargeTime;

        public string PreviewText => ChargeMode switch
        {
            ChargeMode.ChargeByCapacity => $"Current: {ChargeCurrent} A, Cutoff: {ChargeCutoffVoltage} V, Capacity: {ChargeCapacityMah} mAh",
            ChargeMode.ChargeByTime => $"Current: {ChargeCurrent} A, Cutoff: {ChargeCutoffVoltage} V, Time: {ChargeTime}",
            _ => $"Current: {ChargeCurrent} A, Cutoff: {ChargeCutoffVoltage} V, Cutoff Current: {CutoffCurrent} A",
        };
    }

    public enum ChargeMode
    {
        ChargeByCapacity,
        ChargeByTime,
        FullCharge
    }
}
