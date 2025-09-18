using System;
using System.Collections.Generic;
using CellManager.Models;

namespace CellManager.Configuration
{
    /// <summary>
    ///     Centralizes IDataErrorInfo validation logic for cell detail bindings so both the view-model and the
    ///     underlying cell model can surface consistent error messages and range checks.
    /// </summary>
    public static class CellDetailValidation
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

        public static bool HasErrors(Cell? cell)
        {
            if (cell is null)
            {
                return true;
            }

            foreach (var property in ValidatedProperties)
            {
                if (!string.IsNullOrEmpty(GetError(cell, property)))
                {
                    return true;
                }
            }

            return false;
        }

        public static string? GetError(Cell? cell, string? columnName)
        {
            if (cell is null)
            {
                return string.Empty;
            }

            var normalizedColumnName = NormalizeColumnName(columnName);

            return normalizedColumnName switch
            {
                nameof(Cell.ModelName) =>
                    string.IsNullOrWhiteSpace(cell.ModelName)
                        ? "Model name is required"
                        : ValidateStringLength(cell, normalizedColumnName),
                nameof(Cell.Manufacturer) =>
                    ValidateStringLength(cell, normalizedColumnName),
                nameof(Cell.SerialNumber) =>
                    ValidateStringLength(cell, normalizedColumnName),
                nameof(Cell.PartNumber) =>
                    ValidateStringLength(cell, normalizedColumnName),
                nameof(Cell.CellType) =>
                    ValidateStringLength(cell, normalizedColumnName),
                nameof(Cell.ExpansionBehavior) =>
                    ValidateStringLength(cell, normalizedColumnName),
                _ => ValidateNumericRange(cell, normalizedColumnName),
            };
        }

        private static string NormalizeColumnName(string? columnName)
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

        private static string? ValidateStringLength(Cell cell, string propertyName)
        {
            if (!CellDetailTextRules.TryGetRule(propertyName, out var rule))
            {
                return null;
            }

            var value = GetStringValue(cell, propertyName);
            return rule.Range.Contains(value) ? null : rule.CreateLengthErrorMessage(value);
        }

        private static string? ValidateNumericRange(Cell cell, string propertyName)
        {
            if (!CellDetailNumericRules.TryGetRule(propertyName, out var rule))
            {
                return null;
            }

            var value = GetNumericValue(cell, propertyName);
            return rule.Range.Contains(value) ? null : rule.CreateRangeErrorMessage(value);
        }

        private static string GetStringValue(Cell cell, string propertyName) => propertyName switch
        {
            nameof(Cell.ModelName) => cell.ModelName ?? string.Empty,
            nameof(Cell.Manufacturer) => cell.Manufacturer ?? string.Empty,
            nameof(Cell.SerialNumber) => cell.SerialNumber ?? string.Empty,
            nameof(Cell.PartNumber) => cell.PartNumber ?? string.Empty,
            nameof(Cell.CellType) => cell.CellType ?? string.Empty,
            nameof(Cell.ExpansionBehavior) => cell.ExpansionBehavior ?? string.Empty,
            _ => string.Empty,
        };

        private static double GetNumericValue(Cell cell, string propertyName) => propertyName switch
        {
            nameof(Cell.RatedCapacity) => cell.RatedCapacity,
            nameof(Cell.NominalVoltage) => cell.NominalVoltage,
            nameof(Cell.SelfDischarge) => cell.SelfDischarge,
            nameof(Cell.MaxVoltage) => cell.MaxVoltage,
            nameof(Cell.CycleLife) => cell.CycleLife,
            nameof(Cell.InitialACImpedance) => cell.InitialACImpedance,
            nameof(Cell.InitialDCResistance) => cell.InitialDCResistance,
            nameof(Cell.EnergyWh) => cell.EnergyWh,
            nameof(Cell.Weight) => cell.Weight,
            nameof(Cell.Diameter) => cell.Diameter,
            nameof(Cell.Thickness) => cell.Thickness,
            nameof(Cell.Width) => cell.Width,
            nameof(Cell.Height) => cell.Height,
            nameof(Cell.ChargingVoltage) => cell.ChargingVoltage,
            nameof(Cell.CutOffCurrent_Charge) => cell.CutOffCurrent_Charge,
            nameof(Cell.MaxChargingCurrent) => cell.MaxChargingCurrent,
            nameof(Cell.MaxChargingTemp) => cell.MaxChargingTemp,
            nameof(Cell.ChargeTempHigh) => cell.ChargeTempHigh,
            nameof(Cell.ChargeTempLow) => cell.ChargeTempLow,
            nameof(Cell.DischargeCutOffVoltage) => cell.DischargeCutOffVoltage,
            nameof(Cell.MaxDischargingCurrent) => cell.MaxDischargingCurrent,
            nameof(Cell.DischargeTempHigh) => cell.DischargeTempHigh,
            nameof(Cell.DischargeTempLow) => cell.DischargeTempLow,
            nameof(Cell.ConstantCurrent_PreCharge) => cell.ConstantCurrent_PreCharge,
            nameof(Cell.PreChargeStartVoltage) => cell.PreChargeStartVoltage,
            nameof(Cell.PreChargeEndVoltage) => cell.PreChargeEndVoltage,
            _ => 0,
        };
    }
}
