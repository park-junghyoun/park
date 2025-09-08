using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class ChargeProfile
    {
        [ObservableProperty] private int _displayId;
    }

    public partial class DischargeProfile
    {
        [ObservableProperty] private int _displayId;
    }

    public partial class RestProfile
    {
        [ObservableProperty] private int _displayId;
    }

    public partial class OCVProfile
    {
        [ObservableProperty] private int _displayId;
    }

    public partial class ECMPulseProfile
    {
        [ObservableProperty] private int _displayId;
    }
}
