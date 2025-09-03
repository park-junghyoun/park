using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class ECMPulseProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _pulseCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _pulseDuration;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _resetTimeAfterPulse;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _samplingRateMs;

        public string PreviewText => $"Current: {PulseCurrent} A, Duration: {PulseDuration} ms, Reset: {ResetTimeAfterPulse} ms, Sample: {SamplingRateMs} ms";
    }
}
