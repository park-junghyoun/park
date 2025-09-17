using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    /// <summary>
    ///     Describes the parameters required to capture an OCV (Open Circuit Voltage) profile.
    /// </summary>
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

        /// <summary>
        ///     Concise textual representation displayed in the UI profile tree.
        /// </summary>
        public string PreviewText => $"I: {DischargeCurrent_OCV} A, Rest: {RestTime_OCV} s, V: {DischargeCutoffVoltage_OCV} V";
    }
}
