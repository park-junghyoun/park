using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CellManager.Configuration;
using CellManager.Models;
using CellManager.Services;

namespace CellManager.ViewModels
{
    public partial class CellDetailsViewModel : ObservableObject, IDataErrorInfo
    {
        private readonly ICellRepository _cellRepository;

        [ObservableProperty]
        private Cell _cell;

        [ObservableProperty]
        private int _displayId;

        public Action<Cell> OnSaveCompleted { get; set; }
        public Action<Cell> OnCancelCompleted { get; set; }

        public RelayCommand<Window> SaveCommand { get; }
        public RelayCommand<Window> CancelCommand { get; }

        public CellDetailsViewModel(ICellRepository cellRepository, Cell cell)
        {
            _cellRepository = cellRepository;
            Cell = cell;
            DisplayId = cell.Id > 0 ? cell.Id : _cellRepository.GetNextCellId();

            SaveCommand = new RelayCommand<Window>(ExecuteSave, CanSave);
            CancelCommand = new RelayCommand<Window>(ExecuteCancel);

            Cell.PropertyChanged += (_, __) => SaveCommand.NotifyCanExecuteChanged();
        }

        private bool CanSave(Window _) => !CellDetailValidation.HasErrors(Cell);

        private void ExecuteSave(Window window)
        {
            _cellRepository.SaveCell(Cell);
            DisplayId = Cell.Id;
            OnSaveCompleted?.Invoke(Cell);
            window.Close();
        }

        private void ExecuteCancel(Window window)
        {
            OnCancelCompleted?.Invoke(Cell);
            window.Close();
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                return CellDetailValidation.GetError(Cell, columnName) ?? string.Empty;
            }
        }
    }
}