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

            var profile = constraints.Profiles.FirstOrDefault(p => string.Equals(p.Type, type.ToString(), StringComparison.OrdinalIgnoreCase));
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

                var constraint = profile.Fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
                if (constraint == null)
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
        }

        internal sealed class ProfileConstraint
        {
            public string Type { get; set; } = string.Empty;
            public List<FieldConstraint> Fields { get; set; } = new();
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
        }
    }

    internal readonly record struct FieldRangeDescription(string Label, string Description);
}
