using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CellManager.Models;
using CellManager.Services;

namespace CellManager.ViewModels
{
    public partial class CellDetailsViewModel : ObservableObject, IDataErrorInfo
    {
        private readonly ICellRepository _cellRepository;

        [ObservableProperty]
        private Cell _cell;

        public Action<Cell> OnSaveCompleted { get; set; }
        public Action<Cell> OnCancelCompleted { get; set; }

        public RelayCommand<Window> SaveCommand { get; }
        public RelayCommand<Window> CancelCommand { get; }

        public CellDetailsViewModel(ICellRepository cellRepository, Cell cell)
        {
            _cellRepository = cellRepository;
            Cell = cell;

            SaveCommand = new RelayCommand<Window>(ExecuteSave, CanSave);
            CancelCommand = new RelayCommand<Window>(ExecuteCancel);

            Cell.PropertyChanged += (_, __) => SaveCommand.NotifyCanExecuteChanged();
        }

        private bool CanSave(Window _) => !HasErrors;

        private void ExecuteSave(Window window)
        {
            _cellRepository.SaveCell(Cell);
            OnSaveCompleted?.Invoke(Cell);
            window.Close();
        }

        private void ExecuteCancel(Window window)
        {
            OnCancelCompleted?.Invoke(Cell);
            window.Close();
        }

        private bool HasErrors =>
            !string.IsNullOrEmpty(this[nameof(Cell.ModelName)]) ||
            !string.IsNullOrEmpty(this[nameof(Cell.RatedCapacity)]) ||
            !string.IsNullOrEmpty(this[nameof(Cell.NominalVoltage)]);

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(Cell.ModelName) or "ModelName" =>
                        string.IsNullOrWhiteSpace(Cell?.ModelName) ? "Model name is required" : null,
                    nameof(Cell.RatedCapacity) or "RatedCapacity" =>
                        Cell?.RatedCapacity <= 0 ? "Rated capacity must be greater than 0" : null,
                    nameof(Cell.NominalVoltage) or "NominalVoltage" =>
                        Cell?.NominalVoltage <= 0 ? "Nominal voltage must be greater than 0" : null,
                    _ => null,
                };
            }
        }
    }
}