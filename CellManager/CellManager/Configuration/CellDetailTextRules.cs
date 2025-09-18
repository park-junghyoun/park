using System;
using System.Collections.Generic;
using System.Globalization;
using CellManager.Models;

namespace CellManager.Configuration
{
    /// <summary>
    ///     Centralizes the allowed text ranges for user editable fields within the Cell Details dialog.
    /// </summary>
    public static class CellDetailTextRules
    {
        private static readonly IReadOnlyDictionary<string, TextFieldRule> Rules =
            new Dictionary<string, TextFieldRule>(StringComparer.Ordinal)
            {
                [nameof(Cell.ModelName)] = new TextFieldRule(
                    nameof(Cell.ModelName),
                    "Model name",
                    new TextRange(minLength: 1, maxLength: 100)),
                [nameof(Cell.Manufacturer)] = new TextFieldRule(
                    nameof(Cell.Manufacturer),
                    "Manufacturer",
                    new TextRange(minLength: 0, maxLength: 100)),
                [nameof(Cell.SerialNumber)] = new TextFieldRule(
                    nameof(Cell.SerialNumber),
                    "Serial number",
                    new TextRange(minLength: 0, maxLength: 100)),
                [nameof(Cell.PartNumber)] = new TextFieldRule(
                    nameof(Cell.PartNumber),
                    "Part number",
                    new TextRange(minLength: 0, maxLength: 100)),
                [nameof(Cell.CellType)] = new TextFieldRule(
                    nameof(Cell.CellType),
                    "Cell type",
                    new TextRange(minLength: 0, maxLength: 60)),
                [nameof(Cell.ExpansionBehavior)] = new TextFieldRule(
                    nameof(Cell.ExpansionBehavior),
                    "Expansion behavior",
                    new TextRange(minLength: 0, maxLength: 500)),
            };

        public static int ModelNameMaxLength => Rules[nameof(Cell.ModelName)].Range.MaxLength;
        public static int ManufacturerMaxLength => Rules[nameof(Cell.Manufacturer)].Range.MaxLength;
        public static int SerialNumberMaxLength => Rules[nameof(Cell.SerialNumber)].Range.MaxLength;
        public static int PartNumberMaxLength => Rules[nameof(Cell.PartNumber)].Range.MaxLength;
        public static int CellTypeMaxLength => Rules[nameof(Cell.CellType)].Range.MaxLength;
        public static int ExpansionBehaviorMaxLength => Rules[nameof(Cell.ExpansionBehavior)].Range.MaxLength;

        public static bool TryGetRule(string propertyName, out TextFieldRule rule)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                rule = default;
                return false;
            }

            return Rules.TryGetValue(propertyName, out rule);
        }

        public static TextFieldRule GetRule(string propertyName)
        {
            if (TryGetRule(propertyName, out var rule))
            {
                return rule;
            }

            throw new KeyNotFoundException($"No text rule defined for property '{propertyName}'.");
        }
    }

    public readonly struct TextRange
    {
        public TextRange(int minLength, int maxLength)
        {
            if (minLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minLength), minLength, "Minimum length cannot be negative.");
            }

            if (maxLength < minLength)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "Maximum length cannot be less than the minimum length.");
            }

            MinLength = minLength;
            MaxLength = maxLength;
        }

        public int MinLength { get; }
        public int MaxLength { get; }

        public bool Contains(string? value)
        {
            int length = string.IsNullOrEmpty(value) ? 0 : value.Length;
            return length >= MinLength && length <= MaxLength;
        }
    }

    public readonly struct TextFieldRule
    {
        public TextFieldRule(string propertyName, string displayName, TextRange range)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Range = range;
        }

        public string PropertyName { get; }
        public string DisplayName { get; }
        public TextRange Range { get; }

        public string CreateLengthErrorMessage()
        {
            if (Range.MaxLength == int.MaxValue)
            {
                return string.Empty;
            }

            if (Range.MinLength > 0)
            {
                return $"{DisplayName} must be between {Range.MinLength} and {Range.MaxLength} characters.";
            }

            return $"{DisplayName} must be {Range.MaxLength} characters or fewer.";
        }

        public string CreateRangeDescription()
        {
            if (Range.MaxLength == int.MaxValue && Range.MinLength == 0)
            {
                return string.Empty;
            }

            if (Range.MinLength == Range.MaxLength)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "Length: {0} characters",
                    Range.MinLength);
            }

            if (Range.MinLength > 0)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "Length: {0}–{1} characters",
                    Range.MinLength,
                    Range.MaxLength);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                "Max length: {0} characters",
                Range.MaxLength);
        }
    }
}
