using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    public partial class RestProfile : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name;

        [ObservableProperty] private double _restTime;
    }
}
