using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class DischargeProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private string _dischargeMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _dischargeCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _dischargeCutoffVoltage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _dischargeCapacityMah;

        public string PreviewText => $"Mode: {DischargeMode}, Current: {DischargeCurrent} A, Cutoff: {DischargeCutoffVoltage} V, Capacity: {DischargeCapacityMah} mAh";
    }
}
