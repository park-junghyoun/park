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

        [ObservableProperty] private double _qmax;
        [ObservableProperty] private double _socStepPercent;
        [ObservableProperty] private double _dischargeCurrent_OCV;
        [ObservableProperty] private double _restTime_OCV;
        [ObservableProperty] private double _dischargeCutoffVoltage_OCV;
    }
}
