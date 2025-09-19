using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     View model used by the duplicate cell dialog to collect identifying fields for the clone.
    /// </summary>
    public partial class DuplicateCellDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanConfirm))]
        private string _modelName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanConfirm))]
        private string _serialNumber;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanConfirm))]
        private string _partNumber;

        public bool CanConfirm =>
            !string.IsNullOrWhiteSpace(ModelName) &&
            !string.IsNullOrWhiteSpace(SerialNumber) &&
            !string.IsNullOrWhiteSpace(PartNumber);
    }
}
