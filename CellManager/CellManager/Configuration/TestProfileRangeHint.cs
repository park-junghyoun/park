using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Markup;
using CellManager.Models.TestProfile;

namespace CellManager.Configuration
{
    [MarkupExtensionReturnType(typeof(string))]
    public class TestProfileRangeHint : MarkupExtension
    {
        public TestProfileType ProfileType { get; set; }
        public string? FieldName { get; set; }
        public string? FieldNames { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var names = CollectFieldNames();
            if (names.Count == 0)
            {
                return string.Empty;
            }

            var descriptions = TestProfileConstraintProvider.GetRangeDescriptions(ProfileType, names);
            if (descriptions.Count == 0)
            {
                return string.Empty;
            }

            if (descriptions.Count == 1)
            {
                return descriptions[0].Description;
            }

            return string.Join(" • ", descriptions.Select(d => $"{d.Label}: {d.Description}"));
        }

        private List<string> CollectFieldNames()
        {
            var results = new List<string>();

            if (!string.IsNullOrWhiteSpace(FieldName))
            {
                results.Add(FieldName);
            }

            if (!string.IsNullOrWhiteSpace(FieldNames))
            {
                var parts = FieldNames.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        results.Add(trimmed);
                    }
                }
            }

            return results;
        }
    }

    internal static class TestProfileConstraintProvider
    {
        private static readonly Lazy<ConstraintData> Cache = new(Load, isThreadSafe: true);

        public static IReadOnlyList<FieldRangeDescription> GetRangeDescriptions(TestProfileType type, IReadOnlyList<string> fieldNames)
        {
            var result = new List<FieldRangeDescription>();
            if (fieldNames.Count == 0)
            {
                return result;
            }

            var constraints = Cache.Value;
            if (constraints.Profiles.Count == 0)
            {
                return result;
            }

            var profile = constraints.FindProfile(type);
            if (profile == null)
            {
                return result;
            }

            foreach (var name in fieldNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (!profile.TryGetField(name, out var constraint))
                {
                    continue;
                }

                var description = constraint.CreateDescription();
                if (!string.IsNullOrEmpty(description))
                {
                    result.Add(new FieldRangeDescription(constraint.Label ?? name, description));
                }
            }

            return result;
        }

        public static IReadOnlyList<string> GetFieldNames(TestProfileType type)
        {
            var profile = Cache.Value.FindProfile(type);
            return profile?.FieldNames ?? Array.Empty<string>();
        }

        public static bool TryGetFieldConstraint(TestProfileType type, string fieldName, out FieldConstraint constraint)
        {
            constraint = null!;
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return false;
            }

            var profile = Cache.Value.FindProfile(type);
            return profile != null && profile.TryGetField(fieldName, out constraint);
        }

        private static ConstraintData Load()
        {
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var filePath = Path.Combine(basePath, "Config", "TestSetupProfileConstraints.json");
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
            public List<ProfileConstraint> Profiles { get; set; } = new();

            public ProfileConstraint? FindProfile(TestProfileType type)
            {
                if (Profiles.Count == 0)
                {
                    return null;
                }

                var typeName = type.ToString();
                foreach (var profile in Profiles)
                {
                    if (string.Equals(profile.Type, typeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return profile;
                    }
                }

                return null;
            }
        }

        internal sealed class ProfileConstraint
        {
            public string Type { get; set; } = string.Empty;
            public List<FieldConstraint> Fields { get; set; } = new();

            private string[]? _fieldNames;
            private Dictionary<string, FieldConstraint>? _fieldLookup;

            public IReadOnlyList<string> FieldNames
            {
                get
                {
                    if (_fieldNames == null)
                    {
                        _fieldNames = Fields.Select(f => f.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();
                    }

                    return _fieldNames;
                }
            }

            public bool TryGetField(string fieldName, out FieldConstraint constraint)
            {
                _fieldLookup ??= Fields
                    .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                    .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

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

            public string? CreateDescription()
            {
                if (string.Equals(Type, "text", StringComparison.OrdinalIgnoreCase))
                {
                    return CreateTextDescription();
                }

                if (string.Equals(Type, "number", StringComparison.OrdinalIgnoreCase))
                {
                    return CreateNumberDescription();
                }

                return string.Empty;
            }

            public string? CreateValidationError(object? value)
            {
                if (string.Equals(Type, "text", StringComparison.OrdinalIgnoreCase))
                {
                    return ValidateText(value);
                }

                if (string.Equals(Type, "number", StringComparison.OrdinalIgnoreCase))
                {
                    return ValidateNumber(value);
                }

                return null;
            }

            private string? CreateTextDescription()
            {
                if (MaxLength is null && MinLength is null)
                {
                    return string.Empty;
                }

                if (MinLength.HasValue && MaxLength.HasValue && MinLength.Value == MaxLength.Value)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "Length: {0} characters",
                        MinLength.Value);
                }

                if (MinLength.HasValue && MaxLength.HasValue)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "Length: {0}–{1} characters",
                        MinLength.Value,
                        MaxLength.Value);
                }

                if (MaxLength.HasValue)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "Max length: {0} characters",
                        MaxLength.Value);
                }

                return string.Format(
                    CultureInfo.CurrentCulture,
                    "Min length: {0} characters",
                    MinLength);
            }

            private string? ValidateText(object? value)
            {
                var text = value?.ToString() ?? string.Empty;
                var label = Label ?? Name;
                var lengthText = text.Length.ToString(CultureInfo.CurrentCulture);

                if (string.IsNullOrEmpty(text))
                {
                    if (MinLength.HasValue && MinLength.Value > 0)
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} is required (current length: {1}).",
                            label,
                            lengthText);
                    }

                    return null;
                }

                if (MinLength.HasValue && text.Length < MinLength.Value)
                {
                    if (MaxLength.HasValue && MaxLength.Value == MinLength.Value)
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} must be exactly {1} characters long (current length: {2}).",
                            label,
                            MinLength.Value,
                            lengthText);
                    }

                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be at least {1} characters long (current length: {2}).",
                        label,
                        MinLength.Value,
                        lengthText);
                }

                if (MaxLength.HasValue && text.Length > MaxLength.Value)
                {
                    if (MinLength.HasValue && MinLength.Value == MaxLength.Value)
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} must be exactly {1} characters long (current length: {2}).",
                            label,
                            MaxLength.Value,
                            lengthText);
                    }

                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be {1} characters or fewer (current length: {2}).",
                        label,
                        MaxLength.Value,
                        lengthText);
                }

                return null;
            }

            private string? CreateNumberDescription()
            {
                var hasMin = Min.HasValue && !double.IsNegativeInfinity(Min.Value);
                var hasMax = Max.HasValue && !double.IsPositiveInfinity(Max.Value);

                if (!hasMin && !hasMax)
                {
                    return string.Empty;
                }

                if (hasMin && hasMax && AreEqual(Min!.Value, Max!.Value))
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "Value: {0}",
                        FormatNumber(Min.Value));
                }

                if (hasMin && hasMax)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "Range: {0} – {1}",
                        FormatNumber(Min!.Value),
                        FormatNumber(Max!.Value));
                }

                if (hasMin)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "≥ {0}",
                        FormatNumber(Min!.Value));
                }

                return string.Format(
                    CultureInfo.CurrentCulture,
                    "≤ {0}",
                    FormatNumber(Max!.Value));
            }

            private string? ValidateNumber(object? value)
            {
                if (value is null)
                {
                    return null;
                }

                if (!TryConvertToDouble(value, out var number))
                {
                    return null;
                }

                if (double.IsNaN(number) || double.IsInfinity(number))
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be a real number (current value: {1}).",
                        Label ?? Name,
                        number.ToString(CultureInfo.CurrentCulture));
                }

                var hasMin = Min.HasValue && !double.IsNegativeInfinity(Min.Value);
                var hasMax = Max.HasValue && !double.IsPositiveInfinity(Max.Value);
                var label = Label ?? Name;
                var actualText = FormatNumber(number);

                if (hasMin && hasMax)
                {
                    if (number < Min!.Value || number > Max!.Value)
                    {
                        if (AreEqual(Min.Value, Max.Value))
                        {
                            return string.Format(
                                CultureInfo.CurrentCulture,
                                "{0} value {1} must be {2}.",
                                label,
                                actualText,
                                FormatNumber(Min.Value));
                        }

                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} value {1} must be between {2} and {3}.",
                            label,
                            actualText,
                            FormatNumber(Min.Value),
                            FormatNumber(Max.Value));
                    }

                    return null;
                }

                if (hasMin && number < Min!.Value)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} value {1} must be at least {2}.",
                        label,
                        actualText,
                        FormatNumber(Min.Value));
                }

                if (hasMax && number > Max!.Value)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} value {1} must be {2} or less.",
                        label,
                        actualText,
                        FormatNumber(Max.Value));
                }

                return null;
            }

            private string FormatNumber(double value)
            {
                if (string.Equals(Format, "integer", StringComparison.OrdinalIgnoreCase))
                {
                    return value.ToString("0", CultureInfo.CurrentCulture);
                }

                if (Precision is int precision && precision >= 0)
                {
                    var decimals = precision > 0 ? new string('#', precision) : string.Empty;
                    var format = precision > 0 ? $"0.{decimals}" : "0";
                    return value.ToString(format, CultureInfo.CurrentCulture);
                }

                return value.ToString("0.###", CultureInfo.CurrentCulture);
            }

            private static bool AreEqual(double left, double right)
            {
                return Math.Abs(left - right) < 0.0000001;
            }

            private static bool TryConvertToDouble(object value, out double number)
            {
                switch (value)
                {
                    case double d:
                        number = d;
                        return true;
                    case float f:
                        number = f;
                        return true;
                    case decimal m:
                        number = (double)m;
                        return true;
                    case int i:
                        number = i;
                        return true;
                    case long l:
                        number = l;
                        return true;
                    case short s:
                        number = s;
                        return true;
                    case byte b:
                        number = b;
                        return true;
                    case null:
                        number = 0;
                        return false;
                    default:
                        if (value is IConvertible convertible)
                        {
                            try
                            {
                                number = convertible.ToDouble(CultureInfo.CurrentCulture);
                                return true;
                            }
                            catch
                            {
                                number = 0;
                                return false;
                            }
                        }

                        number = 0;
                        return false;
                }
            }
        }
    }

    internal readonly record struct FieldRangeDescription(string Label, string Description);
}
