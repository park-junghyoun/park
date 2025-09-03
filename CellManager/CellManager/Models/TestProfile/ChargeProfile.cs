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

        public string PreviewText => $"Current: {ChargeCurrent} A, Cutoff: {ChargeCutoffVoltage} V, Cutoff Current: {CutoffCurrent} A";
    }
}
