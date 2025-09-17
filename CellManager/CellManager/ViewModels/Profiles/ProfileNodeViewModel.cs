using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels.Profiles
{
    /// <summary>
    ///     Base class for profile tree nodes exposed in the test setup preview UI.
    /// </summary>
    public abstract partial class ProfileNodeViewModel : ObservableObject
    {
        [ObservableProperty] private string name;

        protected ProfileNodeViewModel(string name) => this.name = name;
    }
}
