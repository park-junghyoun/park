using System;
using System.Collections.Generic;
using System.Globalization;
using CellManager.Models;

namespace CellManager.Configuration
{
    /// <summary>
    ///     Centralizes the numeric ranges allowed for editable number fields in the Cell Details dialog.
    /// </summary>
    public static class CellDetailNumericRules
    {
        private static readonly IReadOnlyDictionary<string, NumericFieldRule> Rules =
            new Dictionary<string, NumericFieldRule>(StringComparer.Ordinal)
            {
                [nameof(Cell.RatedCapacity)] = new NumericFieldRule(
                    nameof(Cell.RatedCapacity),
                    "Rated capacity (mAh)",
                    new NumericRange(minValue: 1, maxValue: 1_000_000)),
                [nameof(Cell.NominalVoltage)] = new NumericFieldRule(
                    nameof(Cell.NominalVoltage),
                    "Nominal voltage (mV)",
                    new NumericRange(minValue: 1, maxValue: 10_000)),
                [nameof(Cell.SelfDischarge)] = new NumericFieldRule(
                    nameof(Cell.SelfDischarge),
                    "Self-discharge (%/month)",
                    new NumericRange(minValue: 0, maxValue: 100)),
                [nameof(Cell.MaxVoltage)] = new NumericFieldRule(
                    nameof(Cell.MaxVoltage),
                    "Max voltage (mV)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.CycleLife)] = new NumericFieldRule(
                    nameof(Cell.CycleLife),
                    "Cycle life (cycles)",
                    new NumericRange(minValue: 0, maxValue: 1_000_000)),
                [nameof(Cell.InitialACImpedance)] = new NumericFieldRule(
                    nameof(Cell.InitialACImpedance),
                    "Initial AC impedance (mΩ)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.InitialDCResistance)] = new NumericFieldRule(
                    nameof(Cell.InitialDCResistance),
                    "Initial DC resistance (mΩ)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.EnergyWh)] = new NumericFieldRule(
                    nameof(Cell.EnergyWh),
                    "Energy (Wh)",
                    new NumericRange(minValue: 0, maxValue: 100_000)),
                [nameof(Cell.Weight)] = new NumericFieldRule(
                    nameof(Cell.Weight),
                    "Weight (g)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.Diameter)] = new NumericFieldRule(
                    nameof(Cell.Diameter),
                    "Diameter (mm)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.Thickness)] = new NumericFieldRule(
                    nameof(Cell.Thickness),
                    "Thickness (mm)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.Width)] = new NumericFieldRule(
                    nameof(Cell.Width),
                    "Width (mm)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.Height)] = new NumericFieldRule(
                    nameof(Cell.Height),
                    "Height (mm)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.ChargingVoltage)] = new NumericFieldRule(
                    nameof(Cell.ChargingVoltage),
                    "Charging voltage (mV)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.CutOffCurrent_Charge)] = new NumericFieldRule(
                    nameof(Cell.CutOffCurrent_Charge),
                    "Cut-off current (mA)",
                    new NumericRange(minValue: 0, maxValue: 100_000)),
                [nameof(Cell.MaxChargingCurrent)] = new NumericFieldRule(
                    nameof(Cell.MaxChargingCurrent),
                    "Max charging current (mA)",
                    new NumericRange(minValue: 0, maxValue: 100_000)),
                [nameof(Cell.MaxChargingTemp)] = new NumericFieldRule(
                    nameof(Cell.MaxChargingTemp),
                    "Max charging temperature (°C)",
                    new NumericRange(minValue: -100, maxValue: 200)),
                [nameof(Cell.ChargeTempHigh)] = new NumericFieldRule(
                    nameof(Cell.ChargeTempHigh),
                    "Charge temp high (°C)",
                    new NumericRange(minValue: -100, maxValue: 200)),
                [nameof(Cell.ChargeTempLow)] = new NumericFieldRule(
                    nameof(Cell.ChargeTempLow),
                    "Charge temp low (°C)",
                    new NumericRange(minValue: -100, maxValue: 200)),
                [nameof(Cell.DischargeCutOffVoltage)] = new NumericFieldRule(
                    nameof(Cell.DischargeCutOffVoltage),
                    "Discharge cut-off voltage (mV)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.MaxDischargingCurrent)] = new NumericFieldRule(
                    nameof(Cell.MaxDischargingCurrent),
                    "Max discharging current (mA)",
                    new NumericRange(minValue: 0, maxValue: 100_000)),
                [nameof(Cell.DischargeTempHigh)] = new NumericFieldRule(
                    nameof(Cell.DischargeTempHigh),
                    "Discharge temp high (°C)",
                    new NumericRange(minValue: -100, maxValue: 200)),
                [nameof(Cell.DischargeTempLow)] = new NumericFieldRule(
                    nameof(Cell.DischargeTempLow),
                    "Discharge temp low (°C)",
                    new NumericRange(minValue: -100, maxValue: 200)),
                [nameof(Cell.ConstantCurrent_PreCharge)] = new NumericFieldRule(
                    nameof(Cell.ConstantCurrent_PreCharge),
                    "Constant current (mA)",
                    new NumericRange(minValue: 0, maxValue: 100_000)),
                [nameof(Cell.PreChargeStartVoltage)] = new NumericFieldRule(
                    nameof(Cell.PreChargeStartVoltage),
                    "Pre-charge start voltage (mV)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
                [nameof(Cell.PreChargeEndVoltage)] = new NumericFieldRule(
                    nameof(Cell.PreChargeEndVoltage),
                    "Pre-charge end voltage (mV)",
                    new NumericRange(minValue: 0, maxValue: 10_000)),
            };

        public static bool TryGetRule(string propertyName, out NumericFieldRule rule)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                rule = default;
                return false;
            }

            return Rules.TryGetValue(propertyName, out rule);
        }

        public static NumericFieldRule GetRule(string propertyName)
        {
            if (TryGetRule(propertyName, out var rule))
            {
                return rule;
            }

            throw new KeyNotFoundException($"No numeric rule defined for property '{propertyName}'.");
        }
    }

    public readonly struct NumericRange
    {
        public NumericRange(double minValue, double maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "Maximum value cannot be less than the minimum value.");
            }

            MinValue = minValue;
            MaxValue = maxValue;
        }

        public double MinValue { get; }
        public double MaxValue { get; }

        public bool Contains(double value) => value >= MinValue && value <= MaxValue;
    }

    public readonly struct NumericFieldRule
    {
        public NumericFieldRule(string propertyName, string displayName, NumericRange range)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Range = range;
        }

        public string PropertyName { get; }
        public string DisplayName { get; }
        public NumericRange Range { get; }

        public string CreateRangeErrorMessage()
        {
            var minText = FormatValue(Range.MinValue);
            var maxText = FormatValue(Range.MaxValue);

            if (double.IsNegativeInfinity(Range.MinValue) && double.IsPositiveInfinity(Range.MaxValue))
            {
                return string.Empty;
            }

            if (double.IsNegativeInfinity(Range.MinValue))
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} must be {1} or less.", DisplayName, maxText);
            }

            if (double.IsPositiveInfinity(Range.MaxValue))
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} must be {1} or greater.", DisplayName, minText);
            }

            if (Range.MinValue.Equals(Range.MaxValue))
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} must be {1}.", DisplayName, minText);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                "{0} must be between {1} and {2}.",
                DisplayName,
                minText,
                maxText);
        }

        private static string FormatValue(double value)
        {
            return value.ToString("0.###", CultureInfo.CurrentCulture);
        }
    }
}
