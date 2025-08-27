using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels.Profiles
{
    public partial class ChargeProfileViewModel : ProfileNodeViewModel
    {
        [ObservableProperty] private double chargeCurrent;         // A
        [ObservableProperty] private double chargeCutoffVoltage;   // V
        [ObservableProperty] private double cutoffCurrent;         // A

        public ChargeProfileViewModel(string name = "Charge") : base(name)
        {
            chargeCurrent = 1.0;
            chargeCutoffVoltage = 4.2;
            cutoffCurrent = 0.05;
        }
    }
}
