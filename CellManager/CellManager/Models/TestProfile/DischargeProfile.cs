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

        [ObservableProperty] private string _dischargeMode;
        [ObservableProperty] private double _dischargeCurrent;
        [ObservableProperty] private double _dischargeCutoffVoltage;
        [ObservableProperty] private double _dischargeCapacityMah;
    }
}
