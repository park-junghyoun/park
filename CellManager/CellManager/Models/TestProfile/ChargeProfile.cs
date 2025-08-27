using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class ChargeProfile : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name;

        [ObservableProperty] private double _chargeCurrent;
        [ObservableProperty] private double _chargeCutoffVoltage;
        [ObservableProperty] private double _cutoffCurrent;
    }
}
