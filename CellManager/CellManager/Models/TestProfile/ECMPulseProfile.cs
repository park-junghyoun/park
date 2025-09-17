using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    /// <summary>
    ///     Represents settings for ECM pulse measurements used to identify equivalent circuit models.
    /// </summary>
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

        /// <summary>Compact summary displayed in the pulse profile list.</summary>
        public string PreviewText => $"I: {PulseCurrent} A, Dur: {PulseDuration} ms";
    }
}
