using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Input;
using CellManager.Models;
using CellManager.Services;

namespace CellManager.ViewModels
{
    public partial class CellDetailsViewModel : ObservableObject
    {
        private readonly ICellRepository _cellRepository;

        [ObservableProperty]
        private Cell _cell;

        public Action<Cell> OnSaveCompleted { get; set; }
        public Action<Cell> OnCancelCompleted { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public CellDetailsViewModel(ICellRepository cellRepository, Cell cell)
        {
            _cellRepository = cellRepository;
            Cell = cell;

            SaveCommand = new RelayCommand<Window>(ExecuteSave);
            CancelCommand = new RelayCommand<Window>(ExecuteCancel);
        }

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
    }
}