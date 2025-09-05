using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using CellManager.Messages;
using CellManager.Models;
using CellManager.Services;
using CellManager.Views.CellLibary;

namespace CellManager.ViewModels
{
    public partial class CellLibraryViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Cell Library";
        public string IconName { get; } = "Battery";

        [ObservableProperty]
        private bool _isViewEnabled = true;
        private readonly ICellRepository _cellRepository;

        public ObservableCollection<Cell> CellModels { get; } = new();
        public ICollectionView FilteredCells { get; }

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private Cell _selectedCell;

        // (중요) New 중에만 쓰는 임시 편집 모델 — 리스트에는 추가되지 않음
        [ObservableProperty] private Cell _editingCell;

        // 상세 패널 바인딩 대상: New 중이면 EditingCell, 아니면 SelectedCell
        public Cell CurrentCell => EditingCell ?? SelectedCell;

        partial void OnEditingCellChanged(Cell value)
        {
            OnPropertyChanged(nameof(CurrentCell));
            UpdateCanExecutes();
        }
        partial void OnSelectedCellChanged(Cell value)
        {
            OnPropertyChanged(nameof(CurrentCell));
            UpdateCanExecutes();
        }

        // Commands
        public RelayCommand LoadDataCommand { get; }
        public RelayCommand NewCellCommand { get; }
        public RelayCommand SaveCellCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand<Cell> DeleteCellCommand { get; }
        public RelayCommand<Cell> SelectCellCommand { get; }
        public RelayCommand<Cell> OpenDetailsCommand { get; }

        public CellLibraryViewModel(ICellRepository cellRepository)
        {
            _cellRepository = cellRepository;

            LoadDataCommand = new RelayCommand(ExecuteLoadData);
            NewCellCommand = new RelayCommand(StartNewCell, () => EditingCell == null);
            SaveCellCommand = new RelayCommand(SaveCurrent, () => EditingCell != null || SelectedCell != null);
            CancelEditCommand = new RelayCommand(CancelNew, () => EditingCell != null);
            DeleteCellCommand = new RelayCommand<Cell>(DeleteCell, c => EditingCell == null && c != null && c.Id > 0);

            SelectCellCommand = new RelayCommand<Cell>(ExecuteSelectCell);
            OpenDetailsCommand = new RelayCommand<Cell>(ExecuteOpenDetails, c => c != null);

            FilteredCells = CollectionViewSource.GetDefaultView(CellModels);
            FilteredCells.Filter = OnFilter;

            // 외부에서 선택 방송이 오면 로컬 SelectedCell만 갱신 (로드/강제첫선택 X)
            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                if (m.SelectedCell != null && m.SelectedCell != SelectedCell)
                {
                    SelectedCell = m.SelectedCell;
                    FilteredCells.Refresh();
                }
            });

            // 초기 로드 + 기본 선택
            ExecuteLoadData();
            if (SelectedCell == null && CellModels.Count > 0)
            {
                SelectedCell = CellModels[0];
                WeakReferenceMessenger.Default.Send(new CellSelectedMessage(SelectedCell));
            }
        }

        private bool OnFilter(object obj)
        {
            if (obj is not Cell c) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            var q = SearchText.Trim();
            return (c.ModelName?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (c.Manufacturer?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (c.SerialNumber?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (c.PartNumber?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void ExecuteLoadData()
        {
            var prevId = SelectedCell?.Id ?? 0;

            var cells = _cellRepository.LoadCells(); // SELECT ... ORDER BY Id 권장
            CellModels.Clear();
            foreach (var c in cells)
                CellModels.Add(c);

            // 이전 선택 복원
            if (prevId > 0)
            {
                var found = CellModels.FirstOrDefault(x => x.Id == prevId);
                if (found != null) SelectedCell = found;
            }

            FilteredCells.Refresh();
            UpdateCanExecutes();
        }

        // -------- New / Save / Cancel / Delete --------
        private void StartNewCell()
        {
            // 리스트에 추가하지 않고 임시 모델만 생성 → 우측 상세(CurrentCell)로 표시
            var newCell = new Cell
            {
                ModelName = "New Cell",
                Manufacturer = string.Empty,
                SerialNumber = string.Empty,
                PartNumber = string.Empty
            };

            var vm = new CellDetailsViewModel(_cellRepository, newCell);
            vm.OnSaveCompleted += saved =>
            {
                ExecuteLoadData();
                SelectedCell = CellModels.FirstOrDefault(x => x.Id == saved.Id);
                WeakReferenceMessenger.Default.Send(new CellSelectedMessage(SelectedCell));
                WeakReferenceMessenger.Default.Send(new CellAddedMessage(saved));
            };

            var view = new CellDetailsView { DataContext = vm };
            view.ShowDialog();
        }

        private void SaveCurrent()
        {
            // 새로 만들기 저장 흐름
            if (EditingCell != null)
            {
                _cellRepository.SaveCell(EditingCell); // INSERT + last_insert_rowid()로 Id 채우기 (아래 2) 참고)
                var newId = EditingCell.Id;

                ExecuteLoadData(); // 리스트 갱신
                if (newId > 0)
                {
                    SelectedCell = CellModels.FirstOrDefault(x => x.Id == newId);
                    WeakReferenceMessenger.Default.Send(new CellSelectedMessage(SelectedCell));
                }

                EditingCell = null; // 임시 편집 종료
                return;
            }

            // 기존 항목 저장 흐름
            if (SelectedCell != null)
            {
                var id = SelectedCell.Id;
                _cellRepository.SaveCell(SelectedCell); // UPDATE
                ExecuteLoadData();
                if (id > 0)
                {
                    SelectedCell = CellModels.FirstOrDefault(x => x.Id == id);
                    WeakReferenceMessenger.Default.Send(new CellSelectedMessage(SelectedCell));
                }
            }
        }

        private void CancelNew()
        {
            // 임시 편집 폐기 → 리스트/DB 변화 없음
            EditingCell = null;
        }

        private void DeleteCell(Cell cell)
        {
            if (cell == null || cell.Id <= 0) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete the selected cell?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _cellRepository.DeleteCell(cell);
                ExecuteLoadData();

                SelectedCell = (CellModels.Count > 0) ? CellModels[0] : null;
                WeakReferenceMessenger.Default.Send(new CellDeletedMessage(cell));
                FilteredCells.Refresh();
            }

            UpdateCanExecutes();
        }

        // -------- 기타 --------
        private void ExecuteSelectCell(Cell cell)
        {
            if (EditingCell != null) return; // New 중에는 실수로 리스트 선택 바뀌지 않게 잠금
            if (cell == null) return;

            // 토글 버튼을 다시 눌렀을 때 비활성화 상태로 변경
            if (SelectedCell == cell && !cell.IsActive)
            {
                SelectedCell = null;
                cell.IsActive = false;
                WeakReferenceMessenger.Default.Send(new CellSelectedMessage(null));
            }
            else
            {
                SelectedCell = cell;
                cell.IsActive = true; // 선택된 셀은 활성화 표시
                // 다른 셀들은 비활성화
                foreach (var c in CellModels)
                {
                    if (c.Id != cell.Id && c.IsActive)
                        c.IsActive = false;
                }
                WeakReferenceMessenger.Default.Send(new CellSelectedMessage(cell));
            }
            FilteredCells.Refresh();
        }

        private void ExecuteOpenDetails(Cell cell)
        {
            if (cell == null) return;

            var editCopy = new Cell(cell);
            var vm = new CellDetailsViewModel(_cellRepository, editCopy);
            vm.OnSaveCompleted += saved =>
            {
                ExecuteLoadData();
                SelectedCell = CellModels.FirstOrDefault(x => x.Id == saved.Id);
                WeakReferenceMessenger.Default.Send(new CellSelectedMessage(SelectedCell));
            };

            var view = new CellDetailsView { DataContext = vm };
            view.ShowDialog();
        }

        private void UpdateCanExecutes()
        {
            NewCellCommand?.NotifyCanExecuteChanged();
            SaveCellCommand?.NotifyCanExecuteChanged();
            CancelEditCommand?.NotifyCanExecuteChanged();
            DeleteCellCommand?.NotifyCanExecuteChanged();
        }
    }
}
