using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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
        private static readonly IReadOnlyList<string> ValidatedProperties = new[]
        {
            nameof(Cell.ModelName),
            nameof(Cell.RatedCapacity),
            nameof(Cell.NominalVoltage),
            nameof(Cell.Manufacturer),
            nameof(Cell.SerialNumber),
            nameof(Cell.PartNumber),
            nameof(Cell.CellType),
            nameof(Cell.ExpansionBehavior),
            nameof(Cell.SelfDischarge),
            nameof(Cell.MaxVoltage),
            nameof(Cell.CycleLife),
            nameof(Cell.InitialACImpedance),
            nameof(Cell.InitialDCResistance),
            nameof(Cell.EnergyWh),
            nameof(Cell.Weight),
            nameof(Cell.Diameter),
            nameof(Cell.Thickness),
            nameof(Cell.Width),
            nameof(Cell.Height),
            nameof(Cell.ChargingVoltage),
            nameof(Cell.CutOffCurrent_Charge),
            nameof(Cell.MaxChargingCurrent),
            nameof(Cell.MaxChargingTemp),
            nameof(Cell.ChargeTempHigh),
            nameof(Cell.ChargeTempLow),
            nameof(Cell.DischargeCutOffVoltage),
            nameof(Cell.MaxDischargingCurrent),
            nameof(Cell.DischargeTempHigh),
            nameof(Cell.DischargeTempLow),
            nameof(Cell.ConstantCurrent_PreCharge),
            nameof(Cell.PreChargeStartVoltage),
            nameof(Cell.PreChargeEndVoltage)
        };

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

        private bool CanSave(Window _) => !HasErrors;

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

        private bool HasErrors
        {
            get
            {
                foreach (var property in ValidatedProperties)
                {
                    if (!string.IsNullOrEmpty(this[property]))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                var normalizedColumnName = NormalizeColumnName(columnName);

                return normalizedColumnName switch
                {
                    nameof(Cell.ModelName) =>
                        string.IsNullOrWhiteSpace(Cell?.ModelName)
                            ? "Model name is required"
                            : ValidateStringLength(normalizedColumnName),
                    nameof(Cell.Manufacturer) =>
                        ValidateStringLength(normalizedColumnName),
                    nameof(Cell.SerialNumber) =>
                        ValidateStringLength(normalizedColumnName),
                    nameof(Cell.PartNumber) =>
                        ValidateStringLength(normalizedColumnName),
                    nameof(Cell.CellType) =>
                        ValidateStringLength(normalizedColumnName),
                    nameof(Cell.ExpansionBehavior) =>
                        ValidateStringLength(normalizedColumnName),
                    _ => ValidateNumericRange(normalizedColumnName),
                };
            }
        }

        private static string NormalizeColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return string.Empty;
            }

            const string cellPrefix = "Cell.";
            return columnName.StartsWith(cellPrefix, StringComparison.Ordinal)
                ? columnName.Substring(cellPrefix.Length)
                : columnName;
        }

        private string ValidateStringLength(string propertyName)
        {
            if (!CellDetailTextRules.TryGetRule(propertyName, out var rule))
            {
                return null;
            }

            var value = GetStringValue(propertyName);
            return rule.Range.Contains(value) ? null : rule.CreateLengthErrorMessage(value);
        }

        private string ValidateNumericRange(string propertyName)
        {
            if (!CellDetailNumericRules.TryGetRule(propertyName, out var rule))
            {
                return null;
            }

            var value = GetNumericValue(propertyName);
            return rule.Range.Contains(value) ? null : rule.CreateRangeErrorMessage(value);
        }

        private string GetStringValue(string propertyName) => propertyName switch
        {
            nameof(Cell.ModelName) => Cell?.ModelName ?? string.Empty,
            nameof(Cell.Manufacturer) => Cell?.Manufacturer ?? string.Empty,
            nameof(Cell.SerialNumber) => Cell?.SerialNumber ?? string.Empty,
            nameof(Cell.PartNumber) => Cell?.PartNumber ?? string.Empty,
            nameof(Cell.CellType) => Cell?.CellType ?? string.Empty,
            nameof(Cell.ExpansionBehavior) => Cell?.ExpansionBehavior ?? string.Empty,
            _ => string.Empty,
        };

        private double GetNumericValue(string propertyName) => propertyName switch
        {
            nameof(Cell.RatedCapacity) => Cell?.RatedCapacity ?? 0,
            nameof(Cell.NominalVoltage) => Cell?.NominalVoltage ?? 0,
            nameof(Cell.SelfDischarge) => Cell?.SelfDischarge ?? 0,
            nameof(Cell.MaxVoltage) => Cell?.MaxVoltage ?? 0,
            nameof(Cell.CycleLife) => Cell?.CycleLife ?? 0,
            nameof(Cell.InitialACImpedance) => Cell?.InitialACImpedance ?? 0,
            nameof(Cell.InitialDCResistance) => Cell?.InitialDCResistance ?? 0,
            nameof(Cell.EnergyWh) => Cell?.EnergyWh ?? 0,
            nameof(Cell.Weight) => Cell?.Weight ?? 0,
            nameof(Cell.Diameter) => Cell?.Diameter ?? 0,
            nameof(Cell.Thickness) => Cell?.Thickness ?? 0,
            nameof(Cell.Width) => Cell?.Width ?? 0,
            nameof(Cell.Height) => Cell?.Height ?? 0,
            nameof(Cell.ChargingVoltage) => Cell?.ChargingVoltage ?? 0,
            nameof(Cell.CutOffCurrent_Charge) => Cell?.CutOffCurrent_Charge ?? 0,
            nameof(Cell.MaxChargingCurrent) => Cell?.MaxChargingCurrent ?? 0,
            nameof(Cell.MaxChargingTemp) => Cell?.MaxChargingTemp ?? 0,
            nameof(Cell.ChargeTempHigh) => Cell?.ChargeTempHigh ?? 0,
            nameof(Cell.ChargeTempLow) => Cell?.ChargeTempLow ?? 0,
            nameof(Cell.DischargeCutOffVoltage) => Cell?.DischargeCutOffVoltage ?? 0,
            nameof(Cell.MaxDischargingCurrent) => Cell?.MaxDischargingCurrent ?? 0,
            nameof(Cell.DischargeTempHigh) => Cell?.DischargeTempHigh ?? 0,
            nameof(Cell.DischargeTempLow) => Cell?.DischargeTempLow ?? 0,
            nameof(Cell.ConstantCurrent_PreCharge) => Cell?.ConstantCurrent_PreCharge ?? 0,
            nameof(Cell.PreChargeStartVoltage) => Cell?.PreChargeStartVoltage ?? 0,
            nameof(Cell.PreChargeEndVoltage) => Cell?.PreChargeEndVoltage ?? 0,
            _ => 0,
        };
    }
}