using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class OCVProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _qmax;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _socStepPercent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _dischargeCurrent_OCV;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _restTime_OCV;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _dischargeCutoffVoltage_OCV;

        public string PreviewText => $"Qmax: {Qmax}, SOC Step: {SocStepPercent} %, Current: {DischargeCurrent_OCV} A, Rest: {RestTime_OCV} s, Cutoff: {DischargeCutoffVoltage_OCV} V";
    }
}
