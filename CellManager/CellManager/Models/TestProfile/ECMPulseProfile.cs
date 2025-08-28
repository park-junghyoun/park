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

        [ObservableProperty] private double _pulseCurrent;
        [ObservableProperty] private double _pulseDuration;
        [ObservableProperty] private double _resetTimeAfterPulse;
        [ObservableProperty] private double _samplingRateMs;
    }
}
