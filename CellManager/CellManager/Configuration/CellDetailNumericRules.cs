using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CellManager.Models;

namespace CellManager.Configuration
{
    /// <summary>
    ///     Centralizes the numeric ranges allowed for editable number fields in the Cell Details dialog.
    /// </summary>
    public static class CellDetailNumericRules
    {
        private static readonly Lazy<IReadOnlyDictionary<string, NumericFieldRule>> RuleCache =
            new(LoadRules, isThreadSafe: true);

        private static IReadOnlyDictionary<string, NumericFieldRule> Rules => RuleCache.Value;

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

        private static IReadOnlyDictionary<string, NumericFieldRule> LoadRules()
        {
            var fields = CellDetailConstraintProvider.GetAllFields()
                .Where(f => f.IsNumber)
                .ToDictionary(
                    f => f.Name,
                    f => new NumericFieldRule(
                        f.Name,
                        f.DisplayName,
                        new NumericRange(f.Min ?? double.NegativeInfinity, f.Max ?? double.PositiveInfinity)),
                    StringComparer.Ordinal);

            return fields;
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

        public string CreateRangeErrorMessage(double actualValue)
        {
            var minText = FormatValue(Range.MinValue);
            var maxText = FormatValue(Range.MaxValue);
            var actualText = FormatValue(actualValue);

            if (double.IsNegativeInfinity(Range.MinValue) && double.IsPositiveInfinity(Range.MaxValue))
            {
                return string.Empty;
            }

            if (double.IsNegativeInfinity(Range.MinValue))
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} value {1} must be {2} or less.",
                    DisplayName,
                    actualText,
                    maxText);
            }

            if (double.IsPositiveInfinity(Range.MaxValue))
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} value {1} must be {2} or greater.",
                    DisplayName,
                    actualText,
                    minText);
            }

            if (Range.MinValue.Equals(Range.MaxValue))
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} value {1} must be {2}.",
                    DisplayName,
                    actualText,
                    minText);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                "{0} value {1} must be between {2} and {3}.",
                DisplayName,
                actualText,
                minText,
                maxText);
        }

        public string CreateRangeDescription()
        {
            var minText = FormatValue(Range.MinValue);
            var maxText = FormatValue(Range.MaxValue);

            if (double.IsNegativeInfinity(Range.MinValue) && double.IsPositiveInfinity(Range.MaxValue))
            {
                return string.Empty;
            }

            if (double.IsNegativeInfinity(Range.MinValue))
            {
                return string.Format(CultureInfo.CurrentCulture, "≤ {0}", maxText);
            }

            if (double.IsPositiveInfinity(Range.MaxValue))
            {
                return string.Format(CultureInfo.CurrentCulture, "≥ {0}", minText);
            }

            if (Range.MinValue.Equals(Range.MaxValue))
            {
                return string.Format(CultureInfo.CurrentCulture, "Value: {0}", minText);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                "Range: {0} – {1}",
                minText,
                maxText);
        }

        private static string FormatValue(double value)
        {
            return value.ToString("0.###", CultureInfo.CurrentCulture);
        }
    }
}
