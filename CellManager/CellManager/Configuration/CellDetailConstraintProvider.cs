using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CellManager.Configuration
{
    internal static class CellDetailConstraintProvider
    {
        private static readonly Lazy<ConstraintData> Cache = new(Load, isThreadSafe: true);

        public static IReadOnlyList<FieldConstraint> GetAllFields()
        {
            return Cache.Value.Fields;
        }

        public static bool TryGetFieldConstraint(string fieldName, out FieldConstraint constraint)
        {
            constraint = null!;
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return false;
            }

            return Cache.Value.TryGetField(fieldName, out constraint);
        }

        private static ConstraintData Load()
        {
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var filePath = Path.Combine(basePath, "Config", "CellDetailConstraints.json");
                if (!File.Exists(filePath))
                {
                    return new ConstraintData();
                }

                using var stream = File.OpenRead(filePath);
                var data = JsonSerializer.Deserialize<ConstraintData>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return data ?? new ConstraintData();
            }
            catch
            {
                return new ConstraintData();
            }
        }

        internal sealed class ConstraintData
        {
            public List<FieldConstraint> Fields { get; set; } = new();

            private Dictionary<string, FieldConstraint>? _fieldLookup;

            public bool TryGetField(string fieldName, out FieldConstraint constraint)
            {
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    constraint = null!;
                    return false;
                }

                _fieldLookup ??= Fields
                    .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                    .GroupBy(f => f.Name, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

                return _fieldLookup.TryGetValue(fieldName, out constraint);
            }
        }

        internal sealed class FieldConstraint
        {
            public string Name { get; set; } = string.Empty;
            public string? Label { get; set; }
            public string? Type { get; set; }
            public double? Min { get; set; }
            public double? Max { get; set; }
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }
            public string? Unit { get; set; }
            public int? Precision { get; set; }
            public string? Format { get; set; }

            public bool IsText => string.Equals(Type, "text", StringComparison.OrdinalIgnoreCase);
            public bool IsNumber => string.Equals(Type, "number", StringComparison.OrdinalIgnoreCase);

            public string DisplayName => string.IsNullOrWhiteSpace(Label) ? Name : Label;

            public string CreateDescription()
            {
                if (IsText)
                {
                    return CreateTextDescription();
                }

                if (IsNumber)
                {
                    return CreateNumberDescription();
                }

                return string.Empty;
            }

            public string? CreateValidationError(object? value)
            {
                if (IsText)
                {
                    return ValidateText(value);
                }

                if (IsNumber)
                {
                    return ValidateNumber(value);
                }

                return null;
            }

            private string CreateTextDescription()
            {
                var min = MinLength ?? 0;
                var max = MaxLength ?? int.MaxValue;

                if (min == 0 && max == int.MaxValue)
                {
                    return string.Empty;
                }

                if (min == max)
                {
                    return string.Format(CultureInfo.CurrentCulture, "Length: {0} characters", min);
                }

                if (min > 0)
                {
                    return string.Format(CultureInfo.CurrentCulture, "Length: {0}–{1} characters", min, max);
                }

                return string.Format(CultureInfo.CurrentCulture, "Max length: {0} characters", max);
            }

            private string? ValidateText(object? value)
            {
                var text = value?.ToString() ?? string.Empty;
                var min = MinLength ?? 0;
                var max = MaxLength ?? int.MaxValue;
                var label = DisplayName;
                var currentLength = text.Length.ToString(CultureInfo.CurrentCulture);

                if (string.IsNullOrEmpty(text))
                {
                    if (min > 0)
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} is required (current length: {1}).",
                            label,
                            currentLength);
                    }

                    return null;
                }

                if (min > 0 && text.Length < min)
                {
                    if (max == min)
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} must be exactly {1} characters long (current length: {2}).",
                            label,
                            min,
                            currentLength);
                    }

                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be between {1} and {2} characters (current length: {3}).",
                        label,
                        min,
                        max,
                        currentLength);
                }

                if (text.Length > max)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be {1} characters or fewer (current length: {2}).",
                        label,
                        max,
                        currentLength);
                }

                return null;
            }

            private string CreateNumberDescription()
            {
                var min = Min ?? double.NegativeInfinity;
                var max = Max ?? double.PositiveInfinity;

                if (double.IsNegativeInfinity(min) && double.IsPositiveInfinity(max))
                {
                    return string.Empty;
                }

                var minText = FormatNumber(min);
                var maxText = FormatNumber(max);

                if (double.IsNegativeInfinity(min))
                {
                    return string.Format(CultureInfo.CurrentCulture, "≤ {0}", maxText);
                }

                if (double.IsPositiveInfinity(max))
                {
                    return string.Format(CultureInfo.CurrentCulture, "≥ {0}", minText);
                }

                if (min.Equals(max))
                {
                    return string.Format(CultureInfo.CurrentCulture, "Value: {0}", minText);
                }

                return string.Format(CultureInfo.CurrentCulture, "Range: {0} – {1}", minText, maxText);
            }

            private string? ValidateNumber(object? value)
            {
                if (!TryConvertToDouble(value, out var numericValue))
                {
                    return null;
                }

                var min = Min ?? double.NegativeInfinity;
                var max = Max ?? double.PositiveInfinity;
                var label = DisplayName;
                var actualText = FormatNumber(numericValue);
                var minText = FormatNumber(min);
                var maxText = FormatNumber(max);

                if (numericValue < min)
                {
                    if (min.Equals(max))
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} value {1} must be {2}.",
                            label,
                            actualText,
                            minText);
                    }

                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} value {1} must be {2} or greater.",
                        label,
                        actualText,
                        minText);
                }

                if (numericValue > max)
                {
                    if (min.Equals(max))
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} value {1} must be {2}.",
                            label,
                            actualText,
                            maxText);
                    }

                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} value {1} must be {2} or less.",
                        label,
                        actualText,
                        maxText);
                }

                return null;
            }

            private static bool TryConvertToDouble(object? value, out double numericValue)
            {
                switch (value)
                {
                    case null:
                        numericValue = 0;
                        return false;
                    case double d:
                        numericValue = d;
                        return true;
                    case float f:
                        numericValue = f;
                        return true;
                    case decimal m:
                        numericValue = (double)m;
                        return true;
                    case int i:
                        numericValue = i;
                        return true;
                    case long l:
                        numericValue = l;
                        return true;
                    case short s:
                        numericValue = s;
                        return true;
                    case byte b:
                        numericValue = b;
                        return true;
                    case string text when double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var parsed):
                        numericValue = parsed;
                        return true;
                    default:
                        numericValue = 0;
                        return false;
                }
            }

            private string FormatNumber(double value)
            {
                if (double.IsPositiveInfinity(value))
                {
                    return string.IsNullOrWhiteSpace(Unit) ? "∞" : string.Format(CultureInfo.CurrentCulture, "∞ {0}", Unit);
                }

                if (double.IsNegativeInfinity(value))
                {
                    return string.IsNullOrWhiteSpace(Unit) ? "-∞" : string.Format(CultureInfo.CurrentCulture, "-∞ {0}", Unit);
                }

                string format;
                if (!Precision.HasValue)
                {
                    format = "0.###";
                }
                else if (Precision.Value <= 0)
                {
                    format = "0";
                }
                else
                {
                    format = "0." + new string('#', Precision.Value);
                }

                var formatted = value.ToString(format, CultureInfo.CurrentCulture);
                return string.IsNullOrWhiteSpace(Unit) ? formatted : string.Format(CultureInfo.CurrentCulture, "{0} {1}", formatted, Unit);
            }
        }
    }
}
