using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels.Profiles
{
    public abstract partial class ProfileNodeViewModel : ObservableObject
    {
        [ObservableProperty] private string name;

        protected ProfileNodeViewModel(string name) => this.name = name;
    }
}
